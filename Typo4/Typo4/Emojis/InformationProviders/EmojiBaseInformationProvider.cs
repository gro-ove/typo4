using System;
using System.Collections.Generic;
using System.Linq;
using FirstFloor.ModernUI.Helpers;
using Newtonsoft.Json.Linq;

namespace Typo4.Emojis.InformationProviders {
    // For information loaded from here: https://github.com/milesj/emojibase
    public class EmojiBaseInformationProvider : IEmojiInformationProvider {
        private Dictionary<string, EmojiInformation> _dictionary;

        public static bool Test(string data) {
            return data.StartsWith("[") && data.IndexOf("\"hexcode\"", StringComparison.Ordinal) != -1;
        }

        public EmojiBaseInformationProvider(string data) {
            var array = JArray.Parse(data);
            _dictionary = new Dictionary<string, EmojiInformation>();

            void Add(JObject j, bool hasSkinToneAlternatives, out string skinTone) {
                var key = ((string)j["hexcode"]).Replace("-200D", "").Replace("-FE0F", "").ToLowerInvariant();
                j["index"] = _dictionary.Count;
                _dictionary[key] = GetInformation(j, hasSkinToneAlternatives, out skinTone);
            }

            foreach (var obj in array.OfType<JObject>()) {
                var skins = obj["skins"] as JArray;
                var hasColoredAlternatives = false;

                if (skins?.Count > 0) {
                    foreach (var s in skins.OfType<JObject>()) {
                        Add(s, false, out var skinTone);
                        hasColoredAlternatives |= skinTone != null;
                    }
                }

                Add(obj, hasColoredAlternatives, out _);
            }
        }

        private static string GetSkinTone(string hexcode) {
            var index = hexcode.IndexOf("-1F3F", StringComparison.Ordinal);
            if (index == -1) return null;

            var end = index + 5;
            if (end >= hexcode.Length) return null;

            var last = hexcode[end];
            if (last >= 'B' && last <= 'F') {
                return ("1F3F" + last).ToLowerInvariant();
            }

            return null;
        }

        private static string GetCategory(string category) {
            switch (category) {
                case "0":
                    return "People";
                case "1":
                    return "Nature";
                case "2":
                    return "Food";
                case "3":
                    return "Food";
                case "4":
                    return "Travel";
                case "5":
                    return "Activity";
                case "6":
                    return "Hobbies";
                case "7":
                    return "Flags";
                default:
                    return category;
            }
        }

        private static EmojiInformation GetInformation(JObject j, bool hasSkinToneAlternatives, out string skinTone) {
            return new EmojiInformation(
                    (int)j["index"],
                    ((string)j["name"])?.ToLower().ToTitle(),
                    GetCategory((string)j["group"]),
                    skinTone = GetSkinTone((string)j["hexcode"]),
                    hasSkinToneAlternatives,
                    j["tags"]?.ToObject<string[]>(),
                    (string)j["emoji"]);
        }

        public EmojiInformation GetInformation(string id) {
            return _dictionary.TryGetValue(id, out var j) ? j : null;
        }
    }
}