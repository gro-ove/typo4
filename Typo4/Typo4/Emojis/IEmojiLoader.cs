using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Typo4.Emojis {
    public interface IEmojiLoader {
        [CanBeNull]
        byte[] LoadData([NotNull] string id);

        Task<byte[]> LoadDataAsync([NotNull] string id);
    }
}