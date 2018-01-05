using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TypoLib.Replacers {
    /// <inheritdoc />
    /// <summary>
    /// Replaces selected/all/clipboard text with something else.
    /// </summary>
    public interface IReplacer : IDisposable {
        /// <summary>
        /// Initializes module.
        /// </summary>
        /// <param name="dataDirectory">Path to data directory.</param>
        void Initialize([NotNull] string dataDirectory);

        /// <summary>
        /// Main method replacing text.
        /// </summary>
        /// <param name="originalText">Original text to replace; null if selection/text field/clipboard is empty.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>Transformed text or null if there is nothing to enter after transformation.</returns>
        [NotNull, ItemCanBeNull]
        Task<string> ReplaceAsync([CanBeNull] string originalText, CancellationToken cancellation);
    }
}