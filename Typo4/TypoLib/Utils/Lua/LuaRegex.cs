using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace TypoLib.Utils.Lua {
    public class LuaRegex {
        public bool IgnoreCase { get; set; }

        private readonly Dictionary<string, Regex> _cache;

        public LuaRegex() {
            _cache = new Dictionary<string, Regex>();
        }

        private Regex GetRegex([NotNull] string b, bool ignoreCase) {
            var k = ignoreCase + b;
            return _cache.TryGetValue(k, out var r) ? r :
                    (_cache[k] = new Regex(b, ignoreCase ? RegexOptions.Compiled | RegexOptions.IgnoreCase : RegexOptions.Compiled));
        }

        private string Replace([CanBeNull] string a, [CanBeNull] string b, [CanBeNull] string c, bool ignoreCase) {
            return a == null || b == null ? null : GetRegex(b, ignoreCase).Replace(a, c ?? "");
        }

        private string ReplaceCallback([CanBeNull] string a, [CanBeNull] string b, [CanBeNull] Closure c, bool ignoreCase) {
            return a == null || b == null || c == null ? null : GetRegex(b, ignoreCase).Replace(a, x => {
                var args = new object[x.Groups.Count];
                for (var i = 0; i < args.Length; i++) {
                    args[i] = x.Groups[i].Value;
                }
                try {
                    return c.Call(args).String;
                } catch (Exception e) {
                    TypoLogging.NonFatalErrorNotify("Can’t execute script", null, e);
                    return x.Value;
                }
            });
        }

        private bool IsMatch([CanBeNull] string a, [CanBeNull] string b, bool ignoreCase) {
            return a != null && b != null && GetRegex(b, ignoreCase).IsMatch(a);
        }

        public string Replace(string a, string b, string c) {
            return Replace(a, b, c, IgnoreCase);
        }

        public string ReplaceCallback(string a, string b, Closure c) {
            return ReplaceCallback(a, b, c, IgnoreCase);
        }

        public bool IsMatch(string a, string b) {
            return IsMatch(a, b, IgnoreCase);
        }

        public string ReplaceIgnoreCase(string a, string b, string c) {
            return Replace(a, b, c, IgnoreCase);
        }

        public string ReplaceCallbackIgnoreCase(string a, string b, Closure c) {
            return ReplaceCallback(a, b, c, IgnoreCase);
        }

        public bool IsMatchIgnoreCase(string a, string b) {
            return IsMatch(a, b, IgnoreCase);
        }

        public string ReplaceConsiderCase(string a, string b, string c) {
            return Replace(a, b, c, IgnoreCase);
        }

        public string ReplaceCallbackConsiderCase(string a, string b, Closure c) {
            return ReplaceCallback(a, b, c, IgnoreCase);
        }

        public bool IsMatchConsiderCase(string a, string b) {
            return IsMatch(a, b, IgnoreCase);
        }

        public void Clear() {
            _cache.Clear();
        }
    }
}
