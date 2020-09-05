using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TypoLib.Utils;

namespace Typo4.Clipboards {
    public class ClipboardHistory : NotifyPropertyChanged {
        /// <summary>
        /// Allows to ignore user passwords copied to clipboard.
        /// </summary>
        [CanBeNull]
        private readonly PasswordsContainer _passwordsContainer;

        /// <summary>
        /// Recently copied or pinned items.
        /// </summary>
        public ChangeableObservableCollection<ClipboardEntry> Items { get; }

        #region Parameters
        private int _stackSize = ValuesStorage.GetInt("clipboard.stackSize", 20);

        public int StackSize {
            get => _stackSize;
            set {
                if (Equals(value, _stackSize)) return;
                _stackSize = value;
                OnPropertyChanged();
                ValuesStorage.Set("clipboard.stackSize", value);

                while (Items.Count(x => !x.IsPinned) >= StackSize) {
                    Items.Remove(Items.LastOrDefault(x => !x.IsPinned));
                }
            }
        }
        #endregion

        public ClipboardHistory([CanBeNull] PasswordsContainer passwordsContainer) {
            _passwordsContainer = passwordsContainer;
            if (_passwordsContainer != null) {
                _passwordsContainer.PropertyChanged += OnPasswordsContainerPropertyChanged;
            }

            Items = new ChangeableObservableCollection<ClipboardEntry>(LoadValues());
            Items.ItemPropertyChanged += OnItemPropertyChanged;
            Items.CollectionChanged += OnCollectionChanged;
            SetIndices();

            Inputter.ClipboardHold += OnClipboardHold;
            Inputter.ClipboardRelease += OnClipboardRelease;
            ClipboardMonitor.ClipboardChange += OnClipboardChange;
        }

        private void OnPasswordsContainerPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == nameof(_passwordsContainer) && _passwordsContainer?.CheckSubstrings == true) {
                var any = false;
                for (var i = Items.Count - 1; i >= 0; i--) {
                    if (_passwordsContainer?.IsPassword(Items[i].Value) == true) {
                        Items.RemoveAt(i);
                        any = true;
                    }
                }

                if (any) {
                    SaveValues();
                }
            }
        }

        #region List-related parts
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            SetIndices();
        }

        private void SetIndices() {
            for (int i = 0, p = 0, u = 0; i < Items.Count; i++) {
                var x = Items[i];
                x.Index = x.IsPinned ? ++p : ++u;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ClipboardEntry.IsPinned):
                    Items.RefreshFilter((ClipboardEntry)sender);
                    SaveValues();
                    break;
                case nameof(ClipboardEntry.IsRemoved):
                    Items.Remove((ClipboardEntry)sender);
                    SaveValues();
                    break;
                case nameof(ClipboardEntry.IsMarkedAsPassword):
                    var entry = (ClipboardEntry)sender;
                    Items.Remove(entry);

                    _passwordsContainer?.AddPassword(entry.Value);
                    if (_passwordsContainer?.CheckSubstrings == true) {
                        for (var i = Items.Count - 1; i >= 0; i--) {
                            if (_passwordsContainer?.IsPassword(Items[i].Value) == true) {
                                Items.RemoveAt(i);
                            }
                        }
                    }

                    SaveValues();
                    break;
            }
        }
        #endregion

        #region Inputter-compatibility piece
        private static int _clipboardHeld;

        private void OnClipboardHold(object sender, EventArgs eventArgs) {
            ClipboardMonitor.ClipboardChange -= OnClipboardChange;
            _clipboardHeld++;
        }

        private void OnClipboardRelease(object sender, EventArgs eventArgs) {
            _clipboardHeld--;
            try {
                ClipboardMonitor.ClipboardChange += OnClipboardChange;
            } catch (Exception e) {
                Logging.Warning(e);
            }
        }
        #endregion

        /// <summary>
        /// Returns current clipboard text.
        /// </summary>
        [CanBeNull]
        private static string GetClipboardText() {
            try {
                if (!Clipboard.ContainsText()) return null;
                var data = Clipboard.GetText();
                return string.IsNullOrEmpty(data) || data.Length > 20000 ? null : data;
            } catch (Exception e) {
                Logging.Warning(e);
                return null;
            }
        }

        /// <summary>
        /// Handles clipboard changed events.
        /// </summary>
        private void OnClipboardChange(object sender, EventArgs e) {
            if (_clipboardHeld > 0) return;
            (Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher).InvokeAsync(() => Add(GetClipboardText()));
        }

        /// <summary>
        /// Checks if specific clipboard data should be added to the stack.
        /// </summary>
        /// <param name="clipboardText">Clipboard data.</param>
        /// <returns>True if data should be added.</returns>
        private bool IsToAdd(string clipboardText) {
            return clipboardText != null && _passwordsContainer?.IsPassword(clipboardText) != true;
        }

        /// <summary>
        /// Checks if specific data should be added and either adds it or pushes already exising
        /// entry on top.
        /// </summary>
        /// <param name="clipboardText">Clipboard data.</param>
        private void Add(string clipboardText) {
            if (!IsToAdd(clipboardText)) return;

            var existing = Items.FirstOrDefault(x => x.Value == clipboardText);
            if (existing != null) {
                Items.Remove(existing);
                Items.Insert(0, existing);
            } else {
                if (Items.Count(x => !x.IsPinned) >= StackSize) {
                    Items.Remove(Items.LastOrDefault(x => !x.IsPinned));
                }

                Items.Insert(0, new ClipboardEntry(clipboardText));
            }

            SaveValues();
        }

        #region Loading and saving
        private const string KeyList = "clipboard.list";

        private static IEnumerable<ClipboardEntry> LoadValues() {
            try {
                if (ValuesStorage.Contains(KeyList)) {
                    return JsonConvert.DeserializeObject<ClipboardEntry[]>(ValuesStorage.GetEncryptedString(KeyList));
                }
            } catch (Exception e) {
                Logging.Warning(e);
            }

            return new ClipboardEntry[0];
        }

        private void SaveValues() {
            ValuesStorage.SetEncrypted(KeyList, JsonConvert.SerializeObject(Items));
        }
        #endregion
    }
}