using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TypoLib.Replacers.ScriptInterpreters {
    public interface IScriptInterpreter : IDisposable {
        /// <summary>
        /// Initializes module.
        /// </summary>
        /// <param name="scripsDirectory">Path to scripts directory.</param>
        void Initialize([NotNull] string scripsDirectory);

        [NotNull, ItemCanBeNull]
        Task<string> ExecuteAsync([NotNull] string filename, [CanBeNull] string originalText, CancellationToken cancellation);

        bool IsInputSupported([NotNull] string filename);
    }
}