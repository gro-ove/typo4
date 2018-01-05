
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;
using Typo4.Emojis.InformationProviders;
using TypoLib.Utils.Common;

namespace Typo4.Emojis {
    public class Emoji : NotifyPropertyChanged {
        [NotNull]
        private readonly string _id;

        [NotNull]
        private readonly IEmojiLoader _loader;

        public Emoji([NotNull] string id, [NotNull] IEmojiLoader loader, [NotNull] IEmojiInformationProvider informationProvider) {
            _id = id;
            _loader = loader;
            Image = Lazier.Create(LoadImageSource);
            Information = informationProvider.GetInformation(_id);
        }

        private ImageSource LoadImageSource() {
            var data = _loader.LoadData(_id);
            return data == null ? null : BetterImage.LoadBitmapSourceFromBytes(data).BitmapSource;
        }

        private async Task<ImageSource> LoadImageSourceAsync() {
            var data = await _loader.LoadDataAsync(_id).ConfigureAwait(false);
            return data == null ? null : await Task.Run(() => BetterImage.LoadBitmapSourceFromBytes(data).BitmapSource).ConfigureAwait(false);
        }

        public Lazier<ImageSource> Image { get; }
        public EmojiInformation Information { get; }
        public string Value => Information.Output;
        public string Category => Information.Category;

        private static int[] GetSymbols(string value) {
            var result = new List<int>(value.Length * 2);
            for (var i = 0; i < value.Length; i++) {
                if (char.IsHighSurrogate(value, i)) {
                    result.Add(char.ConvertToUtf32(value, i));
                    i++;
                } else {
                    result.Add(value[i]);
                }
            }
            return result.ToArray();
        }

        private int[] _symbols;
        public int[] Symbols => _symbols ?? (_symbols = GetSymbols(Value));
    }
}