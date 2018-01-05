using System;

namespace Typo4.Emojis.InformationProviders {
    public static class InformationProvidersFactory {
        public static IEmojiInformationProvider Create(string data) {
            if (EmojiBaseInformationProvider.Test(data)) return new EmojiBaseInformationProvider(data);
            if (EmojiOneInformationProvider.Test(data)) return new EmojiOneInformationProvider(data);
            throw new NotSupportedException();
        }
    }
}