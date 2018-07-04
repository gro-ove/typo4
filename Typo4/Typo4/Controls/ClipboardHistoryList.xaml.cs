using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using Typo4.Clipboards;
using Typo4.Popups;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using Clipboard = System.Windows.Clipboard;

namespace Typo4.Controls {
    public partial class ClipboardHistoryList : IInsertControl, IDisposable {
        private Inputter _inputter;

        public ClipboardHistoryList(ClipboardHistory history) {
            DataContext = new ViewModel(history);
            InitializeComponent();

            _inputter = new Inputter();
            this.OnActualUnload(Dispose);
        }

        private ViewModel Model => (ViewModel)DataContext;

        public class ViewModel : NotifyPropertyChanged {
            public ClipboardHistory History { get; }

            public BetterListCollectionView Pinned { get; }
            public BetterListCollectionView Recent { get; }

            public ViewModel(ClipboardHistory history) {
                History = history;
                Pinned = new BetterListCollectionView(History.Items) { Filter = x => (x as ClipboardEntry)?.IsPinned == true };
                Recent = new BetterListCollectionView(History.Items) { Filter = x => (x as ClipboardEntry)?.IsPinned != true };
            }
        }

        public string TextToInsert => null;
        public event EventHandler<TextChosenEventArgs> TextChosen;

        public void OnKeyDown(VirtualKeyCodeEventArgs e) {
            if (e.Key >= Keys.D1 && e.Key <= Keys.D9) {
                PasteClipboardEntry(Model.Recent.OfType<ClipboardEntry>().ElementAtOrDefault(e.Key - Keys.D1));
            } else if (e.Key >= Keys.F1 && e.Key <= Keys.F24) {
                PasteClipboardEntry(Model.Pinned.OfType<ClipboardEntry>().ElementAtOrDefault(e.Key - Keys.F1));
            } else {
                return;
            }

            e.Handled = true;
            KeyboardListener.IgnoreNextReleased(e.Key);
        }

        public void OnKeyUp(VirtualKeyCodeEventArgs e) {}

        private void OnClick(object sender, RoutedEventArgs e) {
            if (((FrameworkElement)sender).DataContext is ClipboardEntry item) {
                PasteClipboardEntry(item);
            }
        }

        private void PasteClipboardEntry([CanBeNull] ClipboardEntry item) {
            if (item == null) return;

            TextChosen?.Invoke(this, new TextChosenEventArgs(null));
            Clipboard.SetText(item.Value);
            _inputter.PasteFromClipboard().Forget();
        }

        public void Dispose() {
            _inputter.Dispose();
        }
    }
}
