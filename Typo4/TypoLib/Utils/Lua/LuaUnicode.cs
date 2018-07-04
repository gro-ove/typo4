using System;

namespace TypoLib.Utils.Lua {
    public class LuaUnicode {
        public LuaUnicode() {}

        public string ToLower(string a) {
            return a.ToLower();
        }

        public string ToUpper(string a) {
            return a.ToUpper();
        }

        public int IndexOf(string a, string b, int startIndex = 0) {
            return a.IndexOf(b, startIndex, StringComparison.InvariantCulture);
        }

        public int IndexOfIgnoreCase(string a, string b, int startIndex = 0) {
            return a.IndexOf(b, startIndex, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
