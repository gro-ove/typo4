/*using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EdgeJs;
using TypoLib.Utils;*/

namespace TypoLib.Replacers.ScriptInterpreters {
    /*public class JsInterpreter : IScriptInterpreter {
        public void Initialize(string scripsDirectory) {}

        [MethodImpl(MethodImplOptions.NoInlining)]
        private async Task<string> ExecuteSafeAsync(string js, string originalText) {
            var func = Edge.Func(js);
            TypoLogging.Write("Run JS: func=" + js);

            var task = func(originalText);
            TypoLogging.Write("Run JS: task=" + js);

            var ret = await task;
            TypoLogging.Write("Run JS: ret=" + js);

            return ret.ToString();
        }

        public async Task<string> ExecuteAsync(string filename, string originalText, CancellationToken cancellation) {
            try {
                var js = File.ReadAllText(filename);
                if (js.IndexOf("\"edge aware\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                        js.IndexOf("\"edge-aware\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                        js.IndexOf("\"edgeaware\"", StringComparison.OrdinalIgnoreCase) == -1) {
                    js = $"return function (input, callback){{\nconsole.log = function (result){{ callback(null, result); }};\n{js}\n}}";
                }
                TypoLogging.Write("Run JS: command=" + js);
                return await ExecuteSafeAsync(js, originalText);
            } catch (DllNotFoundException) {
                return "[ DLL for JS interpeter is missing ]";
            }
        }

        public bool IsInputSupported(string filename) {
            var js = File.ReadAllText(filename);
            return js.IndexOf("\"without input\"", StringComparison.OrdinalIgnoreCase) == -1 &&
                    js.IndexOf("\"noinput\"", StringComparison.OrdinalIgnoreCase) == -1;
        }

        public void Dispose() { }
    }*/
}