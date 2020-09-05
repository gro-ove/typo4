# typo4
A small tool replacing Caps Lock behavior. With it, you can quickly type various Unicode characters, 
automatically add the to text you’re typing, calculate mathematical expressions and more. Most of it 
is customizable, driven by Lua scripts. It can also keep track of clipboard history and help you pick
an emoji.

### TODO:

- Find a way to improve compatibility with UWP apps?

### Features:

- Press Caps Lock to replace “-” with “—” where appropriate, add proper quotes and more in currently active input area¹:
  - Actual changes are set by Lua-scripts in [Replacements](https://github.com/gro-ove/typo4/tree/master/Typo4Data/Replacements) running in order;
  - If any text is selected, it will be processed (instead of processing all of the text in active input area);
  - Use Caps Lock+Ctrl to alter text that is currently in the clipboard.
- Press Caps Lock+F1…F12 keys to apply an according script from [Scripts](https://github.com/gro-ove/typo4/tree/master/Typo4Data/Scripts):
  - If it’s a Lua script, it’ll run with built-in interpreter [MoonSharp](https://www.moonsharp.org/) for optimal speed;
  - Otherwise, Typo4 will attempt to use a system interpreter;
  - Scripts can generate text to insert or modify selected or all text¹;
  - Add `"noinput"` anywhere in a script to mark that it only generates text, and it would work faster.
  - Without that modifier, selected or all text will be accessible via stdin, and output is expected to be in stdout;
  - Lua scripts instead use `input` variable for input and `return` for the results.
- Press Caps Lock+V to access the clipboard history:
  - Pin entries;
  - Quickly insert them by pressing a corresponding button (digits for last entries, F1…F12 keys for pinned);
  - Exclude passwords via their checksums.
- Press Caps Lock+S to insert an Emoji or several:
  - Emojis are split into groups;
  - Custom sets and definitions;
  - Hold Ctrl to insert several Emojis at once.
- Press any other Caps Lock+… combination for character/text shortcuts in [Bindings/Main.lua](https://github.com/gro-ove/typo4/blob/master/Typo4Data/Bindings/Main.lua):
  - Bind any text to any key;
  - For example, Caps Lock+X inserts “×”.
- Typo4 watches for changes in its Data directory, so you can edit everything live;
- Any keystrokes starting with Caps Lock are being captured, so to toggle Caps Lock status use something like Shift+Caps Lock.

---

¹ App uses keyboard hooks and sends Ctrl+C/Ctrl+A/Ctrl+V keyboard events. As straightforward as it gets, but this way it provides pretty good compatibility with various different input areas. Even some videogames supporting those shortcuts will work. However, UWP apps sometimes bug out, but at least for me quite a few of them bug out on a regular basis anyway.

### License:

MIT.
