using System;
using System.Windows.Forms;
using TypoLib.Utils;
using TypoLib.Utils.Windows;

namespace Typo4 {
    public class RussianWindowsFix : IDisposable {
        private KeyboardListener _keyboard;

        public RussianWindowsFix() {
            _keyboard = new KeyboardListener();
            _keyboard.PreviewKeyDown += OnPreviewKeyDown;
            _keyboard.PreviewKeyUp += OnPreviewKeyUp;
        }

        private void OnPreviewKeyDown(object sender, VirtualKeyCodeEventArgs e) {
            if (e.Key != Keys.RMenu) return;
            e.Handled = true;
            e.SkipMainEvent = true;
            User32.SendInput(new User32.KeyboardInput {
                ScanCode = Inputter.ToScanCode(Keys.LMenu),
                Flags = User32.KeyboardFlag.ScanCode
            });
        }

        private void OnPreviewKeyUp(object sender, VirtualKeyCodeEventArgs e) {
            if (e.Key != Keys.RMenu) return;
            e.Handled = true;
            e.SkipMainEvent = true;
            User32.SendInput(new User32.KeyboardInput {
                ScanCode = Inputter.ToScanCode(Keys.LMenu),
                Flags = User32.KeyboardFlag.ScanCode | User32.KeyboardFlag.KeyUp
            });
        }

        public void Dispose() {
            _keyboard.Dispose();
        }
    }
}