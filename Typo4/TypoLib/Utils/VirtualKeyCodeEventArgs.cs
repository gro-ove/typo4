using System;
using System.Windows.Forms;

namespace TypoLib.Utils {
    /// <summary>
    /// All necessary virtual-key-code-to-enum-value convertions will be done later only if needed.
    /// </summary>
    public class VirtualKeyCodeEventArgs : EventArgs {
        public readonly Keys Key;
        public bool Handled;
        public bool SkipMainEvent;

        public VirtualKeyCodeEventArgs(int virtualKeyCode) {
            Key = (Keys)virtualKeyCode;
        }
    }
}