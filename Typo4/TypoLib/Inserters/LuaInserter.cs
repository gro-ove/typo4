using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using TypoLib.Utils.Lua;

namespace TypoLib.Inserters {
    /// <inheritdoc />
    /// <summary>
    /// Finds a symbol or text associated with pressed combination to insert using main Lut-scripts from Bindings directory.
    /// </summary>
    public class LuaInserter : IInserter {
        private string _directory;
        private IDisposable _watcher;

        public void Initialize(string dataDirectory) {
            _directory = Path.Combine(dataDirectory, "Bindings");
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

        public Task<string> GetAsync(IReadOnlyList<Keys> keys, Keys modifiers, CancellationToken cancellation) {
            var keysString = string.Join("_", keys.Select(x => x.ToString()).OrderBy(x => x));
            var modifiersTable = new Dictionary<string, bool> {
                ["alt"] = modifiers.HasFlag(Keys.Alt),
                ["ctrl"] = modifiers.HasFlag(Keys.Control),
                ["shift"] = modifiers.HasFlag(Keys.Shift),
                ["win"] = modifiers.HasFlag(Keys.LWin) || modifiers.HasFlag(Keys.RWin),
            };

            return Task.FromResult(_lua?.Select(x => x.Call(keysString, modifiersTable).String).FirstOrDefault(x => x != null));
        }

        public void Dispose() {
            _watcher.Dispose();
        }
    }
}