using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TypoLib.Utils;
using TypoLib.Utils.Lua;

namespace TypoLib.Replacers.ScriptInterpreters {
    public class LuaInterpreter : IScriptInterpreter {
        public void Initialize(string scripsDirectory) {}

        public Task<string> ExecuteAsync(string filename, string originalText, CancellationToken cancellation) {
            try {
                var state = ScriptExtension.CreateState();
                state.Globals["input"] = originalText;
                return Task.FromResult(state.DoFile(filename).String);
            } catch (Exception e) {
                TypoLogging.Write(e);
                throw;
            }
        }

        public bool IsInputSupported(string filename) {
            var script = File.ReadAllText(filename);
            return script.IndexOf("\"without input\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    script.IndexOf("\"insert mode\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    script.IndexOf("\"insert-only\"", StringComparison.OrdinalIgnoreCase) == -1;
        }

        public void Dispose() { }
    }
}