using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EdgeJs;
using TypoLib.Utils;

namespace TypoLib.Replacers.ScriptInterpreters {
    public class JsInterpreter : IScriptInterpreter {
        public void Initialize(string scripsDirectory) {}

        public async Task<string> ExecuteAsync(string filename, string originalText, CancellationToken cancellation) {
            var js = File.ReadAllText(filename);
            if (js.IndexOf("\"edge aware\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    js.IndexOf("\"edge-aware\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    js.IndexOf("\"edgeaware\"", StringComparison.OrdinalIgnoreCase) == -1) {
                js = $"return function (input, callback){{\nconsole.log = function (result){{ callback(null, result); }};\n{js}\n}}";
            }

            return (await Edge.Func(js)(originalText)).ToString();
        }

        public bool IsInputSupported(string filename) {
            var js = File.ReadAllText(filename);
            return js.IndexOf("\"without input\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    js.IndexOf("\"noinput\"", StringComparison.OrdinalIgnoreCase) == -1;
        }

        public void Dispose() { }
    }
}