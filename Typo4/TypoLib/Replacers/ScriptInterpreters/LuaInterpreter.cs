using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using TypoLib.Utils.Lua;

namespace TypoLib.Replacers.ScriptInterpreters {
    public class LuaInterpreter : IScriptInterpreter {
        public void Initialize(string scripsDirectory) {}

        private static Dictionary<string, string> _cachedScripts;
        private static IDisposable _watcher;

        private static string GetScript(string filename) {
            if (_cachedScripts == null) {
                _cachedScripts = new Dictionary<string, string>();
                _watcher = DirectoryWatcher.WatchDirectory(Path.GetDirectoryName(filename), e => _cachedScripts.Clear());
            }
            if (_cachedScripts.TryGetValue(filename, out var result)) {
                return result;
            }
            return _cachedScripts[filename] = File.ReadAllText(filename);
        }

        public Task<string> ExecuteAsync(string filename, string originalText, CancellationToken cancellation) {
            try {
                var state = ScriptExtension.CreateState();
                state.Globals["input"] = originalText;
                return Task.FromResult(state.DoString(GetScript(filename).Replace(@"{INLINED_INPUT}", originalText)).CastToString());
            } catch (Exception e) {
                TypoLogging.Write(e);
                throw;
            }
        }

        public bool IsInputSupported(string filename) {
            var script = GetScript(filename);
            return script.IndexOf("\"noinput\"", StringComparison.OrdinalIgnoreCase) == -1;
        }

        public void Dispose() { }
    }
}