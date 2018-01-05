using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using TypoLib.Inserters;
using TypoLib.Replacers;
using TypoLib.Utils;
using TypoLib.Utils.Common;

namespace TypoLib {
    /// <summary>
    /// Main controller.
    /// </summary>
    public class Typo : IDisposable, INotifyPropertyChanged {
        private readonly string _dataDirectory;

        private readonly KeyboardListener _keyboardListener;
        private readonly CapsLockListener _capsLockListener;
        private readonly Inputter _inputter;

        private readonly List<IReplacer> _replacers;
        private readonly List<IInserter> _inserters;

        private readonly IReplacer _typografReplacer;
        private readonly ScriptReplacer _scriptReplacer;

        public Typo(string dataDirectory) {
            _dataDirectory = dataDirectory;

            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyDown += OnKeyboardKeyDown;
            _capsLockListener = new CapsLockListener { IdleTimeout = TimeSpan.FromSeconds(10) };
            _capsLockListener.CapsLockStrokeEvent += OnCapsLockStrokeEvent;
            _inputter = new Inputter();

            _replacers = new List<IReplacer>();
            _inserters = new List<IInserter>();

            AddReplacer(new LuaReplacer());
            AddInserter(new LuaInserter());

            _typografReplacer = new TypografReplacer();
            _typografReplacer.Initialize(_dataDirectory);

            _scriptReplacer = new ScriptReplacer();
            _scriptReplacer.Initialize(_dataDirectory);
        }

        public bool IsDebugFormActive {
            get => _capsLockListener.IsDebugFormActive;
            set => _capsLockListener.IsDebugFormActive = value;
        }

        public void AddReplacer(IReplacer replacer) {
            replacer.Initialize(_dataDirectory);
            _replacers.Add(replacer);
        }

        public void AddInserter(IInserter inserter) {
            inserter.Initialize(_dataDirectory);
            _inserters.Add(inserter);
        }

        private async Task<string> ReplaceTextCallback(string originalText, CancellationToken cancellation) {
            var result = originalText;
            foreach (var replacer in _replacers) {
                result = await replacer.ReplaceAsync(result, cancellation);
                if (cancellation.IsCancellationRequested) return result;
            }
            return result;
        }

        private CancellationTokenSource _currentProcessCancellation;

        private void OnKeyboardKeyDown(object sender, VirtualKeyCodeEventArgs e) {
            if (e.Handled) return;

            // The idea here is that if app waits for inserter or replacer to calculate a value, and at that point user presses a new key,
            // current operation is getting cancelled
            _currentProcessCancellation?.Cancel();
        }

        private IDisposable AsyncOperation(out CancellationToken cancellation) {
            // Cancel current operation if any
            _currentProcessCancellation?.Cancel();

            // Create a new cancellation token source
            var cancellationTokenSource = new CancellationTokenSource();

            // Set it as current operation
            _currentProcessCancellation = cancellationTokenSource;

            // And return its token
            cancellation = cancellationTokenSource.Token;

            // At the end of operation, …
            return new ActionAsDisposable(() => {
                // If current operation is still the one created here
                if (ReferenceEquals(_currentProcessCancellation, cancellationTokenSource)) {
                    // Set it as cancelled
                    _currentProcessCancellation = null;
                }
            });
        }

        private async Task ProcessAsync([NotNull] IReadOnlyList<Keys> keys, Keys modifiers) {
            switch (keys.Count) {
                case 0:
                    using (AsyncOperation(out var cancellation)) {
                        await _inputter.ReplaceAsync(ReplaceTextCallback, modifiers, cancellation);
                    }
                    break;

                case 1:
                    if (keys[0] >= Keys.F1 && keys[0] <= Keys.F24) {
                        using (AsyncOperation(out var cancellation)) {
                            var scriptId = keys[0] - Keys.F1 + 1; // Script #1 for F1

#if DEBUG
                            TypoLogging.Write("Script: " + scriptId);
#endif

                            var callback = _scriptReplacer.GetReplaceTextCallback(scriptId);

#if DEBUG
                            TypoLogging.Write("Callback: " + callback);
#endif

                            if (_scriptReplacer.IsInputSupported(scriptId)) {
#if DEBUG
                                TypoLogging.Write("Input is supported");
#endif

                                await _inputter.ReplaceAsync(callback, modifiers, cancellation);
                            } else {
#if DEBUG
                                TypoLogging.Write("Input is not supported");
#endif

                                var textToInsert = await callback(null, cancellation);
#if DEBUG
                                TypoLogging.Write("Text to insert: " + textToInsert);
#endif
                                if (textToInsert != null && !cancellation.IsCancellationRequested) {
                                    _inputter.Insert(textToInsert);
                                }
                            }
                        }
                        break;
                    }

                    if (keys[0] == Keys.PageUp) {
                        using (AsyncOperation(out var cancellation)) {
                            await _inputter.ReplaceAsync(_typografReplacer.ReplaceAsync, modifiers, cancellation);
                        }
                        break;
                    }

                    goto default;

                default:
                    using (AsyncOperation(out var cancellation)) {
                        string toInsert = null;
                        foreach (var inserter in _inserters) {
                            toInsert = await inserter.GetAsync(keys, modifiers, cancellation);
                            if (toInsert != null || cancellation.IsCancellationRequested) break;
                        }

                        if (toInsert != null) {
                            _inputter.Insert(toInsert);
                        }
                    }
                    break;
            }
        }

        private async void OnCapsLockStrokeEvent(object sender, CapsLockStrokeEventArgs e) {
            try {
                await ProcessAsync(e.Keys, e.Modifiers);
            } catch (Exception ex) {
                TypoLogging.NonFatalErrorNotify("Can’t process stroke", null, ex);
            }
        }

        public void Dispose() {
            _keyboardListener.Dispose();
            _capsLockListener.Dispose();
            _replacers.DisposeEverything();
            _inserters.DisposeEverything();
            _inputter.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}