using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using TypoLib.Utils.Lua;

namespace TypoLib.Replacers {
    /// <inheritdoc />
    /// <summary>
    /// Replaces selected/all/clipboard text using main Lut-scripts from Replacements directory.
    /// </summary>
    public class LuaReplacer : IReplacer {
        private string _directory;
        private IDisposable _watcher;

        public void Initialize(string dataDirectory) {
            _directory = Path.Combine(dataDirectory, "Replacements");
            FileUtils.EnsureDirectoryExists(_directory);

            _watcher = DirectoryWatcher.WatchDirectory(_directory, Reload);
            Reload(null);
        }

        [CanBeNull]
        private Closure[] _lua;

        private void Reload(string filename) {
            if (filename?.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) == false) return;
            _lua = Directory.GetFiles(_directory, "*.lua").Select(x => {
                try {
                    return ScriptExtension.CreateState().DoString(File.ReadAllText(x), codeFriendlyName: Path.GetFileName(x)).Function;
                } catch (Exception e) {
                    TypoLogging.NonFatalErrorNotify("Can’t execute script", null, e);
                    return null;
                }
            }).NonNull().ToArray();
        }

        public Task<string> ReplaceAsync(string originalText, CancellationToken cancellation) {
            return Task.FromResult(_lua?.Aggregate(originalText, (current, closure) => closure.Call(current).String));
        }

        public void Dispose() {
            _watcher.Dispose();
        }
    }
}