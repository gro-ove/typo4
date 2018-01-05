using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Attached;
using Typo4.Emojis;
using Typo4.Popups;
using TypoLib.Utils;

namespace Typo4.Controls {
    public partial class EmojisList : IInsertControl {
        private readonly EmojisStorage _emojisStorage;

        private void SetVirtualizationMode(bool enabled) {
            if (enabled) {
                List.Style = (Style)FindResource("VirtualizingItemsControl");
                List.ItemsPanel = (ItemsPanelTemplate)FindResource("VirtualizingTilePanel");
            } else {
                List.ItemsPanel = (ItemsPanelTemplate)FindResource("BasePanel");
            }
        }

        public EmojisList(EmojisStorage emojisStorage) {
            _emojisStorage = emojisStorage;
            InitializeComponent();
            SetVirtualizationMode(true);
        }

        private EmojisList(IEnumerable<Emoji> filteredList, bool virtualizationMode = true) {
            FilteredList = filteredList;
            InitializeComponent();
            SetVirtualizationMode(virtualizationMode);
        }

        private string _filter;

        public string Filter {
            get => _filter;
            set {
                _filter = value;
                List.SetValue(SaveScroll.KeyProperty, "emoji.scroll:" + value);
            }
        }

        public IEnumerable<Emoji> FilteredList { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            DataContext = FilteredList != null ? new ViewModel(FilteredList) : new ViewModel(_emojisStorage, Filter);
        }

        private ViewModel Model => (ViewModel)DataContext;

        public class ViewModel : NotifyPropertyChanged {
            public ViewModel(EmojisStorage emojisStorage, string filter) {
                Emojis = new BetterListCollectionView(emojisStorage.Emojis) {
                    Filter = o => o is Emoji emoji && emoji.Category == filter && emoji.Information.SkinTone == null
                };
            }

            public ViewModel(IEnumerable<Emoji> filteredList) {
                Emojis = new BetterListCollectionView(filteredList.ToList());
            }

            public BetterListCollectionView Emojis { get; }
        }

        public string TextToInsert => null; // (List.SelectedItem as Emoji)?.Value;
        public event EventHandler<TextChosenEventArgs> TextChosen;

        private void OnClick(object sender, MouseButtonEventArgs e) {
            if (((FrameworkElement)sender).DataContext is Emoji emoji) {
                TextChosen?.Invoke(this, new TextChosenEventArgs(emoji.Value ?? "?"));
            }
        }

        private ModernPopup _popup;

        private void OnContextMenu(object sender, MouseButtonEventArgs e) {
            if (((FrameworkElement)sender).DataContext is Emoji emoji && emoji.Information.SkinTone == null && _emojisStorage != null) {
                var colored = _emojisStorage.GetSkinColoredVersions(emoji).ToList();
                if (colored.Count > 0) {
                    var subList = new EmojisList(colored, false);
                    subList.TextChosen += (o, args) => {
                        TextChosen?.Invoke(o, args);
                        if (_popup != null) {
                            _popup.IsOpen = false;
                        }
                    };

                    if (_popup != null) {
                        _popup.IsOpen = false;
                    }

                    _popup = new ModernPopup {
                        Content = subList,
                        PlacementTarget = (FrameworkElement)sender,
                        IsOpen = true,
                        Padding = new Thickness(0)
                    };
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            if (_popup != null) {
                _popup.IsOpen = false;
            }
        }

        public void OnKeyDown(VirtualKeyCodeEventArgs e) {}
        public void OnKeyUp(VirtualKeyCodeEventArgs e) {}
    }
}
