using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Typo4.Clipboards {
    [JsonObject(MemberSerialization.OptIn)]
    public class ClipboardEntry : NotifyPropertyChanged {
        [NotNull, JsonProperty("value")]
        public string Value { get; }

        private string _displayValue, _shortValue;

        [NotNull]
        public string DisplayValue => _displayValue ?? (_displayValue = BbCodeBlock.Encode(Value));

        [NotNull]
        public string ShortValue => _shortValue ?? (_shortValue = DisplayValue.Replace("\r", "").Replace("\n", "[color=\"#4cFFFFFF\"]⮒[/color]"));

        [JsonConstructor]
        public ClipboardEntry([NotNull] string value, bool isPinned = false) {
            Value = value;
            _isPinned = isPinned;
        }

        private bool _isPinned;

        [JsonProperty("isPinned")]
        public bool IsPinned {
            get => _isPinned;
            set {
                if (Equals(value, _isPinned)) return;
                _isPinned = value;
                OnPropertyChanged();
            }
        }

        private int _index;

        public int Index {
            get => _index;
            set {
                if (Equals(value, _index)) return;
                _index = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _togglePinnedCommand;

        public DelegateCommand TogglePinnedCommand => _togglePinnedCommand ?? (_togglePinnedCommand = new DelegateCommand(() => {
            IsPinned = !IsPinned;
        }));

        private bool _isRemoved;

        public bool IsRemoved {
            private get => _isRemoved;
            set {
                if (Equals(value, _isRemoved)) return;
                _isRemoved = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _removeCommand;

        public DelegateCommand RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand(() => {
            IsRemoved = true;
        }));

        private bool _isMarkedAsPassword;

        public bool IsMarkedAsPassword {
            private get => _isMarkedAsPassword;
            set {
                if (Equals(value, _isMarkedAsPassword)) return;
                _isMarkedAsPassword = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _markAsPasswordCommand;

        public DelegateCommand MarkAsPasswordCommand => _markAsPasswordCommand ?? (_markAsPasswordCommand = new DelegateCommand(() => {
            IsMarkedAsPassword = true;
        }));
    }
}