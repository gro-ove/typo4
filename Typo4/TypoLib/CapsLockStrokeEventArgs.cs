using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TypoLib {
    /// <summary>
    /// Caps Lock keystroke event arguments.
    /// </summary>
    public class CapsLockStrokeEventArgs : EventArgs {
        /// <summary>
        /// List of keys pressed apart from modifiers, in order of them being pressed.
        /// </summary>
        public IReadOnlyList<Keys> Keys { get; }

        /// <summary>
        /// Pressed modifiers.
        /// </summary>
        public Keys Modifiers { get; }

        public CapsLockStrokeEventArgs(IReadOnlyList<Keys> keys, Keys modifiers) {
            Keys = keys;
            Modifiers = modifiers;
        }
    }
}