# Typo 4
A small app extending Caps Lock behavior. With its help, you can quickly type various Unicode characters, 
access recently copied pieces of text, calculate mathematical expressions, insert emojis and more. Everything 
is fully customizable, with Lua/Node.JS scripts.

### TODO:

- Sometimes, rarely, 6.Random.js script crashes app, figure out why;
- Find a way to improve compatibility with UWP apps;
- Avoid accidental change of caps locking status more reliably;
- Search for emojis.

### Features:

- Press “Caps Lock” to quickly tune text in selected input area, replacing “-” with “—” where appropriate, adding proper quotes and more:
  - Processing is defined by Lua scripts in “Replacements/” folder;
  - Scripts run in strict order;
  - To tune only a part of some text, select it first;
  - Press “Caps Lock+Ctrl” to instead process the text which is currently in the clipboard.
- Press “Caps Lock+F1…F12” keys to run a script from “Scripts/” folder:
  - Scripts can be written either in Lua or Node.JS;
  - Scripts can either generate some new text, or modify selected or all text;
  - With Node.JS, add `"edge-aware"` flag so Typo4 would expect to see a callback function instead of an external Node.JS interpreter.
  - With Node.JS, add `"noinput"` flag to specify that this script only generates new text.
- Press “Caps Lock+V” to access the clipboard history;
  - Pin entries;
  - Quickly paste an item by pressing a corresponding button;
  - Exclude passwords via their checksums if needed.
- Press “Caps Lock+S” to insert an Emoji or several:
  - Groups and names;
  - Switch between various sets and definitions in Typo 4 settings;
  - Hold “Ctrl” to insert several Emojis at once.
- Press any other “Caps Lock+…” combination for a character or a piece of text in “Bindings/Main.lua”:
  - Bind any text to any key;
  - For instance, by default “Caps Lock+X” is an alias for “×”, quite convenient.
- No need to restart an app when any script is changed: it watches for changes in its “Data/” folder;
- Any keystrokes starting with “Caps Lock” is being captured by Typo 4;
- If you want to toggle “Caps Lock” status, use something like “Shift+Caps Lock”.

### Some details:

- For text transformations, it simply sends Ctrl+C/Ctrl+A/Ctrl+V keystrokes and modifies clipboard content to alter the text. As simple as it gets, but this way, it works with any app as long as it recognizes those shortcuts;
- There were three more iterations of that app, first one was made with AutoHotkey. All buggy too.

### License:

MIT.
