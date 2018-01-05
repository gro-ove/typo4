using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Threading;
using JetBrains.Annotations;
using TypoLib.Debug;
using TypoLib.Utils;

namespace TypoLib {
    /// <summary>
    /// Listens for Caps Lock combinations or single Caps Lock presses. Combinations could be everything from CapsLock+Ctrl
    /// to CapsLock+D1 or even CapsLock+D1+D2+… Basically, simply collects all keys pressed until one of them is being
    /// de-pressed, and then reports a new Caps Lock stroke event.
    /// </summary>
    public class CapsLockListener : IDisposable {
        /// <summary>
        /// Caps Lock or something else, key from which keystrokes should be started.
        /// </summary>
        public Keys StrokeStartKey { get; }

        /// <summary>
        /// Keyboard listener: this thing deals with all hook-related stuff.
        /// </summary>
        [NotNull]
        private readonly KeyboardListener _listener;

        /// <summary>
        /// Timer to reset after a period of inactivity.
        /// </summary>
        [NotNull]
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// List of pressed keys, in order.
        /// </summary>
        [NotNull]
        private readonly List<Keys> _pressed = new List<Keys>();

        public CapsLockListener(Keys strokeStartKey = Keys.Capital) {
            StrokeStartKey = strokeStartKey;
            _listener = new KeyboardListener();
            _listener.KeyDown += OnKeyDown;
            _listener.KeyUp += OnKeyUp;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// After this period of inactivity, pressed keys will be reset.
        /// </summary>
        public TimeSpan IdleTimeout {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        /// <summary>
        /// Timer ticked, now it’s time to reset state.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs eventArgs) {
            Reset();
            _timer.IsEnabled = false;
        }

        /// <summary>
        /// Debug form which is used to shown current keystroke above all other windows for testing.
        /// </summary>
        [CanBeNull]
        private DebugForm _debugForm;

        /// <summary>
        /// Is debug form visible?
        /// </summary>
        public bool IsDebugFormActive {
            get => _debugForm != null;
            set {
                if (_debugForm != null != value) {
                    if (_debugForm != null) {
                        _debugForm.Dispose();
                        _debugForm = null;
                    } else {
                        _debugForm = new DebugForm();
                    }
                }
            }
        }

        /// <summary>
        /// Print message on debug form and in logs.
        /// </summary>
        /// <param name="message">Message to print.</param>
        private void Out([NotNull] string message) {
            // TypoLogging.Write(message);
            _debugForm?.Out(message);
        }

        /// <summary>
        /// Time of a previous event. Assuming human can’t press keys too quickly.
        /// </summary>
        private DateTime _previousEvent;

        /// <summary>
        /// It’s unlikely that human is going to make 50 keypresses per second.
        /// </summary>
        /// <returns></returns>
        private bool OnEvent() {
            var now = DateTime.Now;
            var passed = now - _previousEvent;
            _previousEvent = now;
            return passed.TotalMilliseconds > 5;
        }

        /// <summary>
        /// Changes to false as soon as Caps Lock compination was actioned, to avoid consequent inputs when other keys are being
        /// depressed.
        /// </summary>
        private bool _anyKeyJustWasPressed;

        /// <summary>
        /// Called when any key is pressed.
        /// </summary>
        private void OnKeyDown(object sender, VirtualKeyCodeEventArgs e) {
            if (!OnEvent()) {
                Reset();
                return;
            }

            var c = e.Key;
            Out($"OnKeyDown(): {c} ({string.Join("+", _pressed)})");

            // Ignore all that
            if (c > (Keys)255 || !Enum.IsDefined(typeof(Keys), c)) return;

            // If it’s a first key pressed and it’s not Caps Lock, ~~ignore~~ do not ignore: this way, next keystroke
            // will start only when current keys will be unpressed
            // if (_pressed.Count == 0 && k != StrokeStartKey) return;

            // If key was already pressed, ignore
            if (_pressed.Contains(c)) {
                // But mark event as handled if Caps Lock stroke is being made
                e.Handled |= _pressed[0] == StrokeStartKey;
                Out($"Already pressed: {c} ({string.Join("+", _pressed)})");
                return;
            }

            // Put key in pressed list
            _pressed.Add(c);

            // Only care for Caps Lock strokes
            if ((_pressed.Count == 0 ? c : _pressed[0]) != StrokeStartKey) return;

            // Debug output
            Out($"Input: {string.Join("+", _pressed)} (c: {c}, p: {_anyKeyJustWasPressed})");

            // Mark event as handled to stop keypress propagation
            e.Handled = true;
            _anyKeyJustWasPressed = true;

            // Reset state after a delay
            _timer.Start();
        }

        /// <summary>
        /// Called when any key is unpressed.
        /// </summary>
        private void OnKeyUp(object sender, VirtualKeyCodeEventArgs e) {
            if (!OnEvent()) {
                Reset();
                return;
            }

            var c = e.Key;
            Out($"OnKeyUp(): {c} ({string.Join("+", _pressed)})");

            // Ignore all that
            if (c > (Keys)255 || !Enum.IsDefined(typeof(Keys), c)) return;

            // If this key wasn’t pressed before, exit & reset just in case
            if (!_pressed.Contains(c)) {
                Out($"Not pressed: {c}! {string.Join("+", _pressed)} (p: {_anyKeyJustWasPressed})");
                Reset();
                return;
            }

            // If previous event was a key press and current keystroke is starting with Caps Lock, register it!
            if (_anyKeyJustWasPressed && _pressed[0] == StrokeStartKey) {
                // Debug output
                Out($"Register: {string.Join("+", _pressed)} (c: {c}, p: {_anyKeyJustWasPressed})");

                // Register keystroke
                try {
                    RaiseCapsLockStrokeEvent(FilterModifiers(_pressed, out var modifiers), modifiers);
                } catch (Exception ex) {
                    TypoLogging.Write(ex);
                }
            }

            // Remove after Input() call!
            _pressed.Remove(c);
            _anyKeyJustWasPressed = false;

            Out($"After removal: {string.Join("+", _pressed)}");

            // Reset state after a delay
            if (_pressed.Count > 0) {
                _timer.Start();
            } else {
                _timer.Stop();
            }
        }

        /// <summary>
        /// Reset state in case something is hung up.
        /// </summary>
        private void Reset() {
            Out("Reset");
            _anyKeyJustWasPressed = false;
            _pressed.Clear();
        }

        /// <summary>
        /// Raise the event with caught data.
        /// </summary>
        /// <param name="keys">Keys in order they were pressed.</param>
        /// <param name="modifiers">Modifiers.</param>
        private void RaiseCapsLockStrokeEvent(IReadOnlyList<Keys> keys, Keys modifiers) {
            CapsLockStrokeEvent?.Invoke(this, new CapsLockStrokeEventArgs(keys, modifiers));
        }

        /// <summary>
        /// Caps Lock keystroke event.
        /// </summary>
        public event EventHandler<CapsLockStrokeEventArgs> CapsLockStrokeEvent;

        /// <summary>
        /// Extract all modifiers from pressed keys list into a separate variable and filter out list.
        /// </summary>
        /// <param name="pressed">List of pressed keys.</param>
        /// <param name="modifiers">Extracted modifiers.</param>
        /// <returns>Filtered list.</returns>
        private static IReadOnlyList<Keys> FilterModifiers(List<Keys> pressed, out Keys modifiers) {
            var keys = new List<Keys>(pressed.Count - 1);
            modifiers = 0;

            // Skip 1 since first item is CapsLock aka StrokeStartKey
            for (var i = pressed.Count - 1; i >= 1; i--) {
                var key = pressed[i];
                switch (key) {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        modifiers |= Keys.Control;
                        break;

                    case Keys.LMenu:
                    case Keys.RMenu:
                        modifiers |= Keys.Alt;
                        break;

                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        modifiers |= Keys.Shift;
                        break;

                    default:
                        // Order in which keys were pressed does matter
                        keys.Add(key);
                        break;
                }
            }

            return keys;
        }

        /// <summary>
        /// Disposal.
        /// </summary>
        public void Dispose() {
            _listener.Dispose();
            _debugForm?.Dispose();
        }
    }
}