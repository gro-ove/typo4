using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FirstFloor.ModernUI.Helpers;
using Newtonsoft.Json.Linq;

namespace Typo4.Emojis.InformationProviders {
    // For information loaded from https://www.emojicopy.com
    public class EmojiOneInformationProvider : IEmojiInformationProvider {
        private Dictionary<string, EmojiInformation> _dictionary;

        public static bool Test(string data) {
            return data.StartsWith("{") && data.IndexOf("\"shortname_alternates\"", StringComparison.Ordinal) != -1;
        }

        public EmojiOneInformationProvider(string data) {
            var jobj = JObject.Parse(data);
            _dictionary = new Dictionary<string, EmojiInformation>();

            foreach (var x in jobj) {
                var name = x.Key;
                var value = (JObject)x.Value;
                _dictionary[name] = GetInformation(value);

            }
        }

        private static string GetCategory(string category) {
            switch (category) {
                case "people":
                    return "People";
                default:
                    return category;
            }
        }

        private static string GetEmoji(string output) {
            var s = new StringBuilder();

            void Piece(int previousPosition, int separatorPosition) {
                if (previousPosition >= separatorPosition) return;
                var u = int.Parse(output.Substring(previousPosition, separatorPosition - previousPosition), NumberStyles.HexNumber);
                s.Append(char.ConvertFromUtf32(u));
            }

            var p = 0;
            for (var i = 0; i < output.Length; i++) {
                var c = output[i];
                if (c == '-') {
                    Piece(p, i);
                    p = i + 1;
                }
            }

            Piece(p, output.Length);
            return s.ToString();
        }

        private static EmojiInformation GetInformation(JObject j) {
            var skinTone = (string)j["diversity"];
            var output = (string)j["code_points"]["output"];
            return new EmojiInformation(
                    (int)j["order"],
                    ((string)j["name"])?.ToLower().ToTitle(),
                    GetCategory((string)j["category"]),
                    skinTone == output ? null : skinTone,
                    (j["diversities"] as JArray)?.Count > 1,
                    j["keywords"]?.ToObject<string[]>(),
                    GetEmoji(output));
        }

        public EmojiInformation GetInformation(string id) {
            return _dictionary.TryGetValue(id, out var j) ? j : null;
        }
    }
}