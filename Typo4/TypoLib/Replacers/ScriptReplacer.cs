using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TypoLib.Replacers.ScriptInterpreters;
using TypoLib.Utils;

namespace TypoLib.Replacers {
    public class ScriptReplacer : IReplacer {
        private string _scriptsDirectory;

        public void Initialize(string dataDirectory) {
            _scriptsDirectory = Path.Combine(dataDirectory, "Scripts");
            if (!Directory.Exists(_scriptsDirectory)) {
                Directory.CreateDirectory(_scriptsDirectory);
            }

            Register(".lua", new LuaInterpreter());
            Register(".exe", new ExternalInterpreter(null));
            //Register(".js", new ExternalInterpreter("node.exe"));
            Register(".js", new JsInterpreter());
            Register(".py", new ExternalInterpreter("python.exe"));
            Register(".pl", new ExternalInterpreter("perl.exe"));
            Register(".sh", new ExternalInterpreter("bash.exe"));
            Register(".zsh", new ExternalInterpreter("zsh.exe"));
            Register(".bat", new ExternalInterpreter("cmd", "/C"));
            Register(".cmd", new ExternalInterpreter("cmd", "/C"));
        }

        #region Various interpreters for various extensions
        private Dictionary<string, IScriptInterpreter> _interpreters = new Dictionary<string, IScriptInterpreter>();

        public void Register(string extension, IScriptInterpreter interpreter) {
            _interpreters[extension] = interpreter;
            interpreter.Initialize(_scriptsDirectory);
        }
        #endregion

        /// <summary>
        /// ID of a script to execute, could be either fixed per instance or set dynamically before ReplaceAsync() call.
        /// </summary>
        public int ScriptId { get; set; }

        Task<string> IReplacer.ReplaceAsync(string originalText, CancellationToken cancellation) {
            return ReplaceAsync(ScriptId, originalText, cancellation);
        }

        public ReplaceTextCallback GetReplaceTextCallback(int scriptId) {
            return (originalText, cancellation) => ReplaceAsync(scriptId, originalText, cancellation);
        }

        [ContractAnnotation("=> filename:null, null; => notnull, filename:notnull")]
        private IScriptInterpreter GetInterpreter(int scriptId, out string filename) {
            foreach (var f in Directory.EnumerateFiles(_scriptsDirectory, scriptId + ".*")) {
                if (_interpreters.TryGetValue(Path.GetExtension(f) ?? "", out var interpreter)) {
                    filename = f;
                    return interpreter;
                }
            }

            filename = null;
            return null;
        }

        public bool IsInputSupported(int scriptId) {
            return GetInterpreter(scriptId, out var filename)?.IsInputSupported(filename) != false;
        }

        public Task<string> ReplaceAsync(int scriptId, string originalText, CancellationToken cancellation) {
            try {
                var interpreter = GetInterpreter(scriptId, out var filename);
                if (interpreter != null) {
                    return interpreter.ExecuteAsync(filename, originalText, cancellation);
                }

                TypoLogging.Write($"Supported script not found: {scriptId}");
                return Task.FromResult<string>(null);
            } catch (Exception e) {
                TypoLogging.Write(e);
                throw;
            }
        }

        public void Dispose() {
            _interpreters.DisposeEverything();
        }
    }
}
