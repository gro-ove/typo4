using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Typo4.Controls;
using Typo4.Emojis;

namespace Typo4.Popups {
    public class EmojiPopup : PopupBase {
        private readonly EmojisStorage _storage;

        public EmojiPopup(EmojisStorage storage) {
            _storage = storage;
        }

        protected override IInsertControl GetControl() {
            return new EmojisTabs(_storage);
        }

        protected override Size GetSize() => new Size(663, 480);

        protected override bool TestInput(IReadOnlyList<Keys> keys, Keys modifiers) {
            return keys.Count == 1 && keys[0] == Keys.S && modifiers == Keys.None;
        }
    }
}