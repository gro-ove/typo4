using System;
using System.IO;
using System.IO.Compression;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using TypoLib.Utils;
using TypoLib.Utils.Common;

namespace Typo4.About {
    public sealed class PieceOfInformation : Displayable, IWithId {
        private PieceOfInformation(bool packed, string id, string displayName, string version, string url, string content, bool limited, bool hidden) {
            Id = id;

            DisplayName = displayName;
            Version = version;
            Url = url;
            IsLimited = limited;

            if (packed) {
                _packed = content;
            } else {
                _content = content;
            }
        }

        public PieceOfInformation(string id, string displayName, string version, string url, string content, bool limited, bool hidden)
                : this(true, id, displayName, version, url, content, limited, hidden) {}

        public string Id { get; }

        public string Version { get; }

        public string Url { get; }

        private readonly string _packed;
        private string _content;

        [CanBeNull]
        public string Content => _content ?? (_packed == null ? null : (_content = Unpack(_packed)));

        private static string Unpack(string packed) {
            var bytes = Convert.FromBase64String(packed);
            using (var inputStream = new MemoryStream(bytes)) {
                return new DeflateStream(inputStream, CompressionMode.Decompress).ReadAsStringAndDispose();
            }
        }

        public bool IsLimited { get; }

        public bool IsHidden { get; }
    }
}