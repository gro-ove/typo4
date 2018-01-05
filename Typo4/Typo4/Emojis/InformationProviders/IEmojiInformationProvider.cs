using JetBrains.Annotations;

namespace Typo4.Emojis.InformationProviders {
    public interface IEmojiInformationProvider {
        [CanBeNull]
        EmojiInformation GetInformation([NotNull] string id);
    }
}