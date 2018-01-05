using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Typo4.Emojis {
    public class EmojiInformation {
        [JsonProperty("index")]
        public int Index { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("skinTone")]
        public string SkinTone { get; }

        [JsonProperty("hasSkinToneAlternatives")]
        public bool HasSkinToneAlternatives { get; }

        [JsonProperty("keywords")]
        public string[] Keywords { get; }

        [JsonProperty("category")]
        public string Category { get; }

        [JsonProperty("output")]
        public string Output { get; }

        public EmojiInformation(int index, string name, string category, string skinTone, bool hasSkinToneAlternatives, string[] keywords, string output) {
            Index = index;
            Name = name;
            Category = category;
            Keywords = keywords;
            SkinTone = skinTone;
            HasSkinToneAlternatives = hasSkinToneAlternatives;
            Output = output;
        }

        [JsonConstructor]
        public EmojiInformation(int index, string name, string category, string diversity, string[] keywords, [JsonProperty("code_points")] JObject codePoints) {
            Index = index;
            Name = name;
            Category = category;
            Keywords = keywords;
            SkinTone = diversity;
            Output = (string)codePoints["output"];
        }
    }
}