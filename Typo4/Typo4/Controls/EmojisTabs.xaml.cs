using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Media;
using JetBrains.Annotations;
using Typo4.Emojis;
using Typo4.Popups;
using TypoLib.Utils;
using TypoLib.Utils.Windows;
using Keys = System.Windows.Forms.Keys;

namespace Typo4.Controls {
    public partial class EmojisTabs : IInsertControl {
        public EmojisTabs(EmojisStorage emojisStorage) {
            InitializeComponent();

            foreach (var c in emojisStorage.GetCategories()) {
                var category = c ?? "?";
                Tabs.Links.Add(new Link { Key = category, DisplayName = category.ToLower().ToTitle() });

                var list = new EmojisList(emojisStorage) { Filter = c };
                list.TextChosen += (sender, args) => TextChosen?.Invoke(sender, args);
                DirectLoader.Entries.Add(new DirectContentLoaderEntry { Key = category, Content = list });
            }
        }

        public string TextToInsert => (Tabs.Frame.Content as IInsertControl)?.TextToInsert;
        public event EventHandler<TextChosenEventArgs> TextChosen;

        public void OnKeyDown(VirtualKeyCodeEventArgs e) {
            if (e.Key == Keys.Tab) {
                var selected = Tabs.SelectedSource;
                Tabs.SelectedSource = (
                        User32.IsKeyPressed(Keys.LShiftKey) || User32.IsKeyPressed(Keys.RShiftKey) ?
                                Tabs.Links.Concat(Tabs.Links.TakeWhile(x => x.Source != selected)).LastOrDefault() :
                                Tabs.Links.SkipWhile(x => x.Source != selected).Skip(1).Concat(Tabs.Links).FirstOrDefault()
                        )?.Source ?? selected;
                e.Handled = true;
            } else if (e.Key >= Keys.D1 && e.Key <= Keys.D9 &&
                    User32.IsKeyPressed(Keys.LControlKey) || User32.IsKeyPressed(Keys.RControlKey)) {
                Tabs.SelectedSource = Tabs.Links.ElementAtOrDefault(e.Key - Keys.D1)?.Source ?? Tabs.SelectedSource;
                e.Handled = true;
            }
        }

        public void OnKeyUp(VirtualKeyCodeEventArgs e) {}

        private bool _selectionSet;

        [CanBeNull]
        private Cell _selectionCell;

        [CanBeNull]
        private ListBox _selectionListBox;
        private ScaleTransform _selectionScaleTransform;
        private TranslateTransform _selectionTranslateTransform;
        private EasingFunctionBase _selectionEasingFunction;

        private void InitializeMovingSelectionHighlight() {
            _selectionSet = true;

            _selectionCell = Tabs.FindVisualChildren<Cell>().FirstOrDefault(x => x.Name == "PART_Cell");
            _selectionListBox = _selectionCell?.FindVisualChildren<ListBox>().FirstOrDefault(x => x.Name == "PART_LinkList");
            if (_selectionCell == null || _selectionListBox == null) return;

            SetSelected();
        }

        private void MoveInitializedSelectionHighlight() {
            SetSelected();
        }

        [CanBeNull]
        private Tuple<Point, Size> GetSelected() {
            if (_selectionCell == null || _selectionListBox == null) return null;

            var selected = (ListBoxItem)_selectionListBox.GetItemVisual(_selectionListBox.SelectedItem);
            return selected == null ? null : Tuple.Create(selected.TransformToAncestor(_selectionCell).Transform(new Point(0, 0)),
                    new Size(selected.ActualWidth / _selectionCell.ActualWidth, selected.ActualHeight / _selectionCell.ActualHeight));
        }

        private void SetSelected() {
            var selected = GetSelected();
            if (selected == null) return;

            if (_selectionScaleTransform == null) {
                _selectionScaleTransform = new ScaleTransform { ScaleX = selected.Item2.Width, ScaleY = selected.Item2.Height };
                _selectionTranslateTransform = new TranslateTransform { X = selected.Item1.X, Y = selected.Item1.Y };

                var box = _selectionCell.FindVisualChildren<Border>().FirstOrDefault(x => x.Name == "PART_SelectionBox");
                if (box != null) {
                    box.RenderTransform = new TransformGroup { Children = { _selectionScaleTransform, _selectionTranslateTransform } };
                }
            } else {
                var duration = TimeSpan.FromSeconds(0.2 + (((_selectionTranslateTransform.X - selected.Item1.X).Abs() - 100d) / 500d).Clamp(0d, 0.5d));
                var easing = _selectionEasingFunction ?? (_selectionEasingFunction = (EasingFunctionBase)FindResource("StandardEase"));
                _selectionTranslateTransform.BeginAnimation(TranslateTransform.XProperty,
                        new DoubleAnimation { To = selected.Item1.X, Duration = duration, EasingFunction = easing });
                _selectionTranslateTransform.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation { To = selected.Item1.Y, Duration = duration, EasingFunction = easing });
                _selectionScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty,
                        new DoubleAnimation { To = selected.Item2.Width, Duration = duration, EasingFunction = easing });
                _selectionScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation { To = selected.Item2.Height, Duration = duration, EasingFunction = easing });
            }
        }

        private void MoveSelectionHighlight() {
            try {
                if (_selectionSet) {
                    MoveInitializedSelectionHighlight();
                } else {
                    InitializeMovingSelectionHighlight();
                }
            } catch (Exception e) {
                Logging.Error(e);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            MoveSelectionHighlight();
        }

        private void OnSelectedSourceChanged(object sender, SourceEventArgs e) {
            MoveSelectionHighlight();
        }
    }
}
