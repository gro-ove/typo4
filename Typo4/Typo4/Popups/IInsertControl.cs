using System;
using JetBrains.Annotations;
using TypoLib.Utils;

namespace Typo4.Popups {
    public interface IInsertControl {
        [CanBeNull]
        string TextToInsert { get; }

        event EventHandler<TextChosenEventArgs> TextChosen;

        void OnKeyDown(VirtualKeyCodeEventArgs e);
        void OnKeyUp(VirtualKeyCodeEventArgs e);
    }
}