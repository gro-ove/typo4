using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;
using Typo4.Emojis.InformationProviders;
using TypoLib.Utils;
using TypoLib.Utils.Common;

namespace Typo4.Emojis {
    public class EmojisStorage : NotifyPropertyChanged, IDisposable {
        [NotNull]
        public BetterObservableCollection<Emoji> Emojis { get; }

        [CanBeNull]
        private IEmojiInformationProvider _informationProvider;

        [CanBeNull]
        private EmojiLoader _emojiLoader;

        private void RecreateEmojis() {
            if (_emojiLoader != null && _informationProvider != null) {
                Emojis.ReplaceEverythingBy_Direct(_emojiLoader?.GetList()
                                                               .Select(x => new Emoji(x, _emojiLoader, _informationProvider))
                                                               .Where(x => x.Information != null)
                                                               .OrderBy(x => x.Information.Index));
            } else {
                Emojis.Clear();
            }
        }

        [NotNull]
        public BetterObservableCollection<FileInfo> GraphicsFiles { get; }

        [NotNull]
        public BetterObservableCollection<FileInfo> DescriptionFiles { get; }

        private FileInfo _selectedGraphics = new FileInfo(ValuesStorage.GetString("settings.emojiGraphics") ?? "none"),
                _selectedDescription = new FileInfo(ValuesStorage.GetString("settings.emojiDescription") ?? "none");

        [CanBeNull]
        public FileInfo SelectedGraphics {
            get => _selectedGraphics;
            set {
                if (Equals(value, _selectedGraphics)) return;
                _selectedGraphics = value;
                OnPropertyChanged();

                _emojiLoader = value == null ? null : new EmojiLoader(value.FullName);
                RecreateEmojis();

                ValuesStorage.Set("settings.emojiGraphics", value?.FullName);
            }
        }

        [CanBeNull]
        public FileInfo SelectedDescription {
            get => _selectedDescription;
            set {
                if (Equals(value, _selectedDescription)) return;
                _selectedDescription = value;
                OnPropertyChanged();

                _informationProvider = value == null ? null : InformationProvidersFactory.Create(File.ReadAllText(value.FullName));
                RecreateEmojis();

                ValuesStorage.Set("settings.emojiDescription", value?.FullName);
            }
        }

        private DelegateCommand _viewGraphicsDirectoryCommand;

        public DelegateCommand ViewGraphicsDirectoryCommand => _viewGraphicsDirectoryCommand ?? (_viewGraphicsDirectoryCommand = new DelegateCommand(() => {
            WindowsHelper.ViewDirectory(_graphicsDirectory);
        }));

        private DelegateCommand _viewDescriptionsDirectoryCommand;

        public DelegateCommand ViewDescriptionsDirectoryCommand => _viewDescriptionsDirectoryCommand ?? (_viewDescriptionsDirectoryCommand = new DelegateCommand(() => {
            WindowsHelper.ViewDirectory(_descriptionsDirectory);
        }));

        [NotNull]
        private readonly string _graphicsDirectory, _descriptionsDirectory;

        private IDisposable _graphicsWatcher, _descriptionWatcher;

        public EmojisStorage(string dataDirectory) {
            Emojis = new BetterObservableCollection<Emoji>();

            _graphicsDirectory = Path.Combine(dataDirectory, "Emojis", "Graphics");
            _descriptionsDirectory = Path.Combine(dataDirectory, "Emojis", "Descriptions");

            GraphicsFiles = new BetterObservableCollection<FileInfo>();
            DescriptionFiles = new BetterObservableCollection<FileInfo>();
            RescanGraphics();
            RescanDescriptions();

            _graphicsWatcher = DirectoryWatcher.WatchDirectory(_graphicsDirectory, GraphicsWatcherCallback);
            _descriptionWatcher = DirectoryWatcher.WatchDirectory(_descriptionsDirectory, DescriptionWatcherCallback);
        }

        private void GraphicsWatcherCallback(string s) {
            if (s?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == false) return;
            RescanGraphics();
        }

        private void DescriptionWatcherCallback(string s) {
            if (s?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == false) return;
            RescanDescriptions();
        }

        private void RescanGraphics() {
            FileUtils.EnsureDirectoryExists(_graphicsDirectory);
            GraphicsFiles.ReplaceEverythingBy_Direct(new DirectoryInfo(_graphicsDirectory).GetFiles("*.zip"));
            SelectedGraphics = GraphicsFiles.FirstOrDefault(x => FileUtils.ArePathsEqual(x.FullName, SelectedGraphics?.FullName ?? ""))
                    ?? GraphicsFiles.FirstOrDefault();
        }

        private void RescanDescriptions() {
            FileUtils.EnsureDirectoryExists(_descriptionsDirectory);
            DescriptionFiles.ReplaceEverythingBy_Direct(new DirectoryInfo(_descriptionsDirectory).GetFiles("*.json"));
            SelectedDescription = DescriptionFiles.FirstOrDefault(x => FileUtils.ArePathsEqual(x.FullName, SelectedDescription?.FullName ?? ""))
                    ?? DescriptionFiles.FirstOrDefault();
        }

        public void Dispose() {
            _graphicsWatcher.Dispose();
            _descriptionWatcher.Dispose();
        }

        #region Loading
        private class EmojiLoader : IEmojiLoader {
            private static readonly Regex FixNameRegex = new Regex(@"-(?:200d|fe0f)\b|\.png$", RegexOptions.IgnoreCase);

            private Dictionary<string, byte[]> _data;

            public EmojiLoader(string filename) {
                using (var archive = new ZipArchive(File.OpenRead(filename), ZipArchiveMode.Read, false)) {
                    _data = archive.Entries.Where(x => x.Name.EndsWith(".png")).ToDictionary(x => FixNameRegex.Replace(x.Name, ""), x => x.Open().ReadAsBytesAndDispose());
                }
            }

            public IEnumerable<string> GetList() {
                return _data.Keys;
            }

            public byte[] LoadData(string id) {
                return _data.TryGetValue(id, out var data) ? data : null;
            }

            public Task<byte[]> LoadDataAsync(string id) {
                return Task.FromResult(_data.TryGetValue(id, out var data) ? data : null);
            }
        }
        #endregion

        #region Methods to get images and stuff
        [CanBeNull]
        public ImageSource GetImageSource(string emojiCode) {
            var data = _emojiLoader?.LoadData(emojiCode);
            return data != null ? BetterImage.LoadBitmapSourceFromBytes(data).BitmapSource : null;
        }

        public IEnumerable<string> GetCategories() {
            return Emojis.Select(x => x.Category).Distinct();
        }
        #endregion

        #region Methods for emojis
        private static bool IsColoredSkinModifier(int c) {
            // Source: http://unicode.org/reports/tr51/#Emoji_Modifiers_Table
            return c >= 0x1f3fb && c <= 0x1f3ff;
        }

        private static bool IsGenderModifier(int c) {
            return c == 0x2640 || c == 0x2642;
        }

        private static bool IsGenderPerson(int c) {
            return c == 0x1f468 || c == 0x1f469;
        }

        private static bool IsModifier(int c) {
            return IsColoredSkinModifier(c) || c == 0x200D || c == 0xfe0f;
        }

        public static bool IsColoredVersionOf(Emoji potentiallyColoredEmoji, Emoji baseEmoji) {
            return potentiallyColoredEmoji.Information.SkinTone != null &&
                    potentiallyColoredEmoji.Category == baseEmoji.Category &&
                    potentiallyColoredEmoji.Symbols.Where(x => !IsModifier(x)).SequenceEqual(baseEmoji.Symbols.Where(x => !IsModifier(x)));
        }

        public IEnumerable<Emoji> GetSkinColoredVersions(Emoji emoji) {
            return emoji.Information.SkinTone != null ? new Emoji[0] : Emojis.Where(x => IsColoredVersionOf(x, emoji));
        }
        #endregion
    }
}