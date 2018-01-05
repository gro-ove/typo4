using System;

namespace Typo4.Popups {
    public class TextChosenEventArgs : EventArgs {
        public TextChosenEventArgs(string text, bool? closePopup = null) {
            Text = text;
            ClosePopup = closePopup;
        }

        public string Text { get; }
        public bool? ClosePopup { get; }
    }
}