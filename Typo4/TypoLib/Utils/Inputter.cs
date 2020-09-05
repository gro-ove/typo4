using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using JetBrains.Annotations;
using TypoLib.Utils.Common;
using TypoLib.Utils.Windows;

namespace TypoLib.Utils {
    /// <summary>
    /// Callback to replace text.
    /// </summary>
    /// <param name="originalText">Original text, either currently selected, taken from focused text field or from clipboard.</param>
    [NotNull, ItemCanBeNull]
    public delegate Task<string> ReplaceTextCallback([CanBeNull] string originalText, CancellationToken cancellation);

    /// <summary>
    /// How to replace text.
    /// </summary>
    public enum TextReplacementType {
        /// <summary>
        /// If some bit of text is selected, replace it. Otherwise, replace everything in focused text field.
        /// </summary>
        Auto,

        /// <summary>
        /// Replace everything in focused text field.
        /// </summary>
        All,

        /// <summary>
        /// Replace text in the clipboard.
        /// </summary>
        Clipboard,

        /// <summary>
        /// Replace selected text only.
        /// </summary>
        Selected
    }

    /// <inheritdoc />
    /// <summary>
    /// Handles everything about getting text from current text fields and putting it back or simply adding new symbols
    /// as a regular keyboard. Since, apparently, there is no hint of API which would allow to work with any input text field,
    /// uses clipboard and hotkeys like Ctrl+C and Ctrl+V to do the job. Awful solution, but it works.
    /// </summary>
    /// <remarks>
    /// Call .Dispose() if Inputter is watching for keys (watchForKeys=true).
    /// </remarks>
    public class Inputter : IDisposable {
        /// <summary>
        /// Delay between key presses. Increase if some apps fail to register some keys.
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="watchForKeys">If true, Inputter will watch for</param>
        public Inputter(bool watchForKeys = false) {
            if (watchForKeys) {
                InitializeListener();
            }
        }

        #region Input text using events
        /// <summary>
        /// Insert set of unicode symbols into currently focused text field.
        /// </summary>
        /// <param name="data">Data to enter, unicode. If empty or null, nothing will happen.</param>
        public void Insert([CanBeNull] string data) {
            if (string.IsNullOrEmpty(data)) return;

            var chars = data.ToCharArray();
            for (var i = 0; i < chars.Length; i++) {
                if (i % 3 == 2) {
                    Thread.Sleep(Delay);
                }

                User32.SendInput(
                        new User32.KeyboardInput {
                            ScanCode = chars[i],
                            Flags = User32.KeyboardFlag.Unicode,
                            ExtraInfo = KeyboardListener.AppEventFlag
                        },
                        new User32.KeyboardInput {
                            ScanCode = chars[i],
                            Flags = User32.KeyboardFlag.Unicode | User32.KeyboardFlag.KeyUp,
                            ExtraInfo = KeyboardListener.AppEventFlag
                        });
            }
        }
        #endregion

        #region Replace selected text using clipboard and common shortcuts

        #region Clipboard-related
        /// <summary>
        /// Gets text from the clipboard.
        /// </summary>
        /// <returns>Text or NULL if clipboard is empty.</returns>
        [CanBeNull]
        private static string GetClipboardText() {
            try {
                if (!Clipboard.ContainsText()) return null;
                var data = Clipboard.GetText();
                return string.IsNullOrEmpty(data) ? null : data;
            } catch (Exception e) {
                TypoLogging.Write(e);
                return null;
            }
        }

        private static void SetClipboardTextInner([CanBeNull] string data) {
            try {
                if (string.IsNullOrEmpty(data)) {
                    Clipboard.Clear();
                } else {
                    Clipboard.SetDataObject(data, true, 5, 100);
                }
            } catch (Exception e) {
                TypoLogging.Write(e);
            }
        }

        /// <summary>
        /// Sets text to the clipboard.
        /// </summary>
        /// <param name="data">Data to set; if NULL, clipboard will be cleared instead.</param>
        private static void SetClipboardText([CanBeNull] string data) {
            HoldClipboard();
            SetClipboardTextInner(data);
            ReleaseClipboard();
        }

        /// <summary>
        /// Backups text from the clipboard and returns IDisposable allowing to restore it
        /// later with using(…){ … }.
        /// </summary>
        /// <remarks>
        /// Only text is being restored. Trying to save other data might end up badly. For more
        /// information: https://stackoverflow.com/a/2579846/305865.
        /// </remarks>
        /// <returns>IDisposable restoring text on Dispose().</returns>
        [NotNull]
        private static IDisposable Backup() {
            HoldClipboard();
            var data = GetClipboardText();
            SetClipboardTextInner(null);
            return new ActionAsDisposable(() => {
                SetClipboardTextInner(data);
                ReleaseClipboard();
            });
        }

        private static int _holdCounter;

        private static void HoldClipboard() {
            if (_holdCounter == 0) {
                ClipboardHold?.Invoke(null, EventArgs.Empty);
            }
            ++_holdCounter;
        }

        private static void ReleaseClipboard() {
            --_holdCounter;
            if (_holdCounter == 0) {
                Task.Delay(200).ContinueWith(t => ClipboardRelease?.Invoke(null, EventArgs.Empty));
            }
        }

        public static event EventHandler ClipboardHold;
        public static event EventHandler ClipboardRelease;
        #endregion

        #region Replace selected text using convertation method
        /// <summary>
        /// Gets selected/copied/all text, passes it through provided callback and puts it back.
        /// </summary>
        /// <param name="proc">Callback converting text.</param>
        /// <param name="type">What text to replace: selected/copied/all/auto.</param>
        /// <param name="cancellation">Cancellation token.</param>
        public async Task ReplaceAsync(ReplaceTextCallback proc, TextReplacementType type, CancellationToken cancellation) {
            using (type == TextReplacementType.Clipboard ? null : Backup()) {
                switch (type) {
                    case TextReplacementType.Auto:
                        await PressAsync(Keys.RControlKey, Keys.C);
                        if (!Clipboard.ContainsText()) {
                            await PressAsync(Keys.RControlKey, Keys.A);
                            await PressAsync(Keys.RControlKey, Keys.C);
                        }
                        break;

                    case TextReplacementType.All:
                        await PressAsync(Keys.RControlKey, Keys.A);
                        await PressAsync(Keys.RControlKey, Keys.C);
                        break;

                    case TextReplacementType.Selected:
                        await PressAsync(Keys.RControlKey, Keys.C);
                        break;
                }

                if (cancellation.IsCancellationRequested) return;

#if DEBUG
                var s = Stopwatch.StartNew();
#endif

                try {
                    var replacement = await proc(GetClipboardText(), cancellation);
                    if (cancellation.IsCancellationRequested) return;

                    SetClipboardText(replacement);
                } catch (Exception e) {
                    TypoLogging.NonFatalErrorNotify("Can’t execute script", null, e);
                }

#if DEBUG
                TypoLogging.Write($"{s.Elapsed.TotalMilliseconds:F1} ms");
#endif

                if (type != TextReplacementType.Clipboard) {
                    await PressAsync(Keys.RControlKey, Keys.V);
                }
            }
        }

        /// <summary>
        /// Picks type of text replacement based on pressed modifier keys.
        /// </summary>
        /// <param name="modifiers">Pressed modifier keys.</param>
        /// <returns>Type of text replacement.</returns>
        private static TextReplacementType GetTextReplaceType(Keys modifiers) {
            if (modifiers.HasFlag(Keys.Control)) return TextReplacementType.Clipboard;
            if (modifiers.HasFlag(Keys.Alt)) return TextReplacementType.Selected;
            if (modifiers.HasFlag(Keys.Shift)) return TextReplacementType.All;
            return TextReplacementType.Auto;
        }

        /// <summary>
        /// Gets selected/copied/all text, passes it through provided callback and puts it back.
        /// </summary>
        /// <param name="proc">Callback converting text.</param>
        /// <param name="modifiers">Depending on modifier keys, type of text replacement will be picked.</param>
        /// <param name="cancellation">Cancellation token.</param>
        public Task ReplaceAsync(ReplaceTextCallback proc, Keys modifiers, CancellationToken cancellation) {
            return ReplaceAsync(proc, GetTextReplaceType(modifiers), cancellation);
        }
        #endregion

        #region Simulate a bunch of keys being pressed and unpressed
        /// <summary>
        /// Converts key (virtual key code) to its scan code.
        /// </summary>
        /// <param name="key">Key in question.</param>
        /// <returns>Scan code.</returns>
        public static ushort ToScanCode(Keys key) {
            var virtualKey = (uint)key;
            var scanCode = User32.MapVirtualKey(virtualKey, 0);
            return (ushort)scanCode;
        }

        /// <summary>
        /// Simulates a bunch of keys being pressed in specified order and then unpressed.
        /// </summary>
        /// <param name="values">Keys.</param>
        private async Task PressAsync(params Keys[] values) {
            for (var i = 0; i < values.Length; i++) {
                User32.SendInput(new User32.KeyboardInput {
                    ScanCode = ToScanCode(values[i]),
                    Flags = User32.KeyboardFlag.ScanCode,
                    ExtraInfo = KeyboardListener.AppEventFlag
                });
                await Task.Delay(Delay);
            }

            for (var i = values.Length - 1; i >= 0; i--) {
                User32.SendInput(new User32.KeyboardInput {
                    ScanCode = ToScanCode(values[i]),
                    Flags = User32.KeyboardFlag.ScanCode | User32.KeyboardFlag.KeyUp,
                    ExtraInfo = KeyboardListener.AppEventFlag
                });
                await Task.Delay(Delay);
            }
        }
        #endregion

        #endregion

        #region Miscellaneous methods
        /// <summary>
        /// Paste whatever is in clipboard to focused text field.
        /// </summary>
        public async Task PasteFromClipboard() {
            var revert = await ResetPressedAsync();
            try {
                await PressAsync(Keys.RControlKey, Keys.V);
            } finally {
                if (revert != null) {
                    await revert();
                }
            }
        }
        #endregion

        #region Watch for pressed buttons
        /// <summary>
        /// Release pressed keys and return a function which will re-press them again.
        /// </summary>
        /// <returns>Function which will re-press them again.</returns>
        private async Task<Func<Task>> ResetPressedAsync() {
            TypoLogging.Write(_pressed?.Count);
            if (_pressed == null) return null;

            var pressed = _pressed.ToArray();
            for (var i = pressed.Length - 1; i >= 0; i--) {
                TypoLogging.Write("Unpress: " + pressed[i]);
                User32.SendInput(new User32.KeyboardInput {
                    ScanCode = ToScanCode(pressed[i]),
                    Flags = User32.KeyboardFlag.ScanCode | User32.KeyboardFlag.KeyUp,
                    ExtraInfo = KeyboardListener.AppEventFlag
                });
                await Task.Delay(Delay);
            }

            return async () => {
                for (var i = 0; i < pressed.Length; i++) {
                    TypoLogging.Write("Re-press: " + pressed[i]);
                    User32.SendInput(new User32.KeyboardInput {
                        ScanCode = ToScanCode(pressed[i]),
                        Flags = User32.KeyboardFlag.ScanCode,
                        ExtraInfo = KeyboardListener.AppEventFlag
                    });
                    await Task.Delay(Delay);
                }
            };
        }

        /// <summary>
        /// Keyboard listener: this thing deals with all hook-related stuff.
        /// </summary>
        [CanBeNull]
        private KeyboardListener _keyboard;

        /// <summary>
        /// Timer to reset after a period of inactivity.
        /// </summary>
        [CanBeNull]
        private DispatcherTimer _timer;

        /// <summary>
        /// List of pressed keys, in order.
        /// </summary>
        [CanBeNull]
        private List<Keys> _pressed;

        private void InitializeListener() {
            TypoLogging.Write("Here");
            _pressed = new List<Keys>();
            _keyboard = new KeyboardListener();
            _keyboard.PreviewKeyDown += OnPreviewKeyDown;
            _keyboard.PreviewKeyUp += OnPreviewKeyUp;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// Timer ticked, now it’s time to reset state.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs eventArgs) {
            Reset();

            if (_timer != null) {
                _timer.IsEnabled = false;
            }
        }

        /// <summary>
        /// Reset state in case something is hung up.
        /// </summary>
        private void Reset() {
            _pressed?.Clear();
        }

        private void OnPreviewKeyDown(object sender, VirtualKeyCodeEventArgs e) {
            _pressed?.Add(e.Key);

            // Reset state after a delay
            _timer?.Start();
        }

        private void OnPreviewKeyUp(object sender, VirtualKeyCodeEventArgs e) {
            _pressed?.Remove(e.Key);

            // Reset state after a delay
            if (_pressed?.Count > 0) {
                _timer?.Start();
            } else {
                _timer?.Stop();
            }
        }

        public void Dispose() {
            if (_timer != null) {
                _timer.IsEnabled = false;
            }
            _keyboard?.Dispose();
        }
        #endregion
    }
}