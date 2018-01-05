using System.Collections.Generic;
using System.Windows.Forms;
using Typo4.Clipboards;
using Typo4.Controls;

namespace Typo4.Popups {
    public class ClipboardPopup : PopupBase {
        private readonly ClipboardHistory _history;

        public ClipboardPopup(ClipboardHistory history) {
            _history = history;
        }

        protected override IInsertControl GetControl() {
            return new ClipboardHistoryList(_history);
        }

        protected override bool TestInput(IReadOnlyList<Keys> keys, Keys modifiers) {
            return keys.Count == 1 && keys[0] == Keys.V && modifiers == Keys.None;
        }
    }
}