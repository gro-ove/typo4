using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;
using Typo4.Utils;
using TypoLib.Inserters;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using Keys = System.Windows.Forms.Keys;

namespace Typo4.Popups {
    public abstract class PopupBase : IInserter {
        public void Initialize(string dataDirectory) {}

        [NotNull]
        protected abstract IInsertControl GetControl();

        protected abstract bool TestInput([NotNull] IReadOnlyList<Keys> keys, Keys modifiers);

        protected virtual Size GetSize() => new Size(720, 480);

        private class PopupWrapper : ModernDialog, IDisposable {
            private readonly KeyboardListener _keyboard;
            private readonly Inputter _inputter;
            private readonly IInsertControl _control;

            public PopupWrapper(Size size, IInsertControl control) {
                _keyboard = new KeyboardListener();
                _inputter = new Inputter();
                _control = control;

                Content = control;
                Padding = new Thickness(0, 0, 0, -24);
                Style = (Style)Application.Current.FindResource("PopupDialog");
                Width = size.Width;
                Height = size.Height;
                Buttons = new Control[0];
                Background = new SolidColorBrush(AppearanceManager.Current.AccentColor) { Opacity = 0.6 };
                ShowActivated = false;
                LocationAndSizeKey = control.GetType().Name;

                control.TextChosen += OnControlTextChosen;
                _keyboard.KeyDown += OnKeyDown;
                _keyboard.KeyUp += OnKeyUp;

                MuiSystemAccent.SystemColorsChanged += OnSystemColorsChanged;
            }

            private void OnSystemColorsChanged(object sender, EventArgs eventArgs) {
                Background = new SolidColorBrush(AppearanceManager.Current.AccentColor) { Opacity = 0.6 };
            }

            private void OnControlTextChosen(object sender, TextChosenEventArgs e) {
                if (e.Text != null) {
                    _inputter.Insert(e.Text);
                }

                if (e.ClosePopup == true
                        || e.ClosePopup == null && TypoModel.Instance.ClosePopupsOnInsert && Keyboard.Modifiers != ModifierKeys.Control) {
                    Dispatcher.InvokeAsync(Close);
                }
            }

            private void OnKeyDown(object sender, VirtualKeyCodeEventArgs e) {
                _control.OnKeyDown(e);
                if (e.Handled) return;

                if (Keyboard.Modifiers == ModifierKeys.Control) {
                    if (e.Key == Keys.W || e.Key == Keys.F4) {
                        e.Handled = true;
                        Close();
                    }
                } else {
                    if (e.Key == Keys.Escape) {
                        e.Handled = true;
                        Close();
                    }
                }
            }

            private void OnKeyUp(object sender, VirtualKeyCodeEventArgs e) {
                _control.OnKeyUp(e);
            }

            public void Dispose() {
                _keyboard.Dispose();
                MuiSystemAccent.SystemColorsChanged -= OnSystemColorsChanged;
            }
        }

        private static readonly Dictionary<string, Action> CloseCallbacks = new Dictionary<string, Action>();

        private async Task RunPopup() {
            var key = GetType().Name;

            PopupWrapper[] wrapper = { null };
            var closeCallback = (Action)(() => wrapper[0]?.Close());
            if (CloseCallbacks.TryGetValue(key, out var shown)) shown();

            using (wrapper[0] = new PopupWrapper(GetSize(), GetControl())) {
                CloseCallbacks[key] = closeCallback;
                await wrapper[0].ShowAndWaitAsync();
                if (CloseCallbacks.TryGetValue(key, out var current) && current == closeCallback) {
                    CloseCallbacks.Remove(key);
                }
            }
        }

        public Task<string> GetAsync(IReadOnlyList<Keys> keys, Keys modifiers, CancellationToken cancellation) {
            if (TestInput(keys, modifiers)) {
                RunPopup().Forget();
            }

            return Task.FromResult<string>(null);
        }

        public void Dispose() { }
    }
}