using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace TypoLib.Inserters {
    /// <inheritdoc />
    /// <summary>
    /// Module which finds a symbol or text associated with pressed combination to insert.
    /// </summary>
    public interface IInserter : IDisposable {
        /// <summary>
        /// Initializes module.
        /// </summary>
        /// <param name="dataDirectory">Path to data directory.</param>
        void Initialize([NotNull] string dataDirectory);

        /// <summary>
        /// Finds a symbol or text.
        /// </summary>
        /// <param name="keys">Pressed combination.</param>
        /// <param name="modifiers">Active modifiers.</param>
        /// <returns>Single symbol or text to insert.</returns>
        [NotNull, ItemCanBeNull]
        Task<string> GetAsync([NotNull] IReadOnlyList<Keys> keys, Keys modifiers, CancellationToken cancellation);
    }
}