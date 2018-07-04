# typo4
Tool extending Caps Lock behavior. With it, you can quickly type various Unicode characters, 
view recent clipboard items, calculate mathematical expressions, insert emojis and more. Everything 
is fully customizable, with Lua/Node.JS scripts.

For now, it’s WIP, so if you want to try it, you’ll need to manually copy Typo4Data 
to AppData\Roaming as Typo4.

### TODO:

- Add data into EXE-file to be installed automatically;
- Switch from Enigma VB to Costura;
- Make a first release;
- Sometimes, rarely, 6.Random.js script crashes app, figure out why;
- Find a way to improve compatibility with UWP apps?

### Features:

- Press Caps Lock to quickly tune text you’re typing, replacing “-” with “—” where appropriate, adding proper quotes and more:
  - This behavior is defined by Lua-scripts in Replacements;
  - They run in order, processing text they have;
  - Select some text if you need to tune nothing by it;
  - With Caps Lock+Ctrl, text in the clipboard will be tuned.
- Press Caps Lock+F1…F12 keys to apply an according script from Scripts:
  - Scripts can be either in Lua or Node.JS;
  - They can either just generate some text to insert, or modify selected/all text;
  - With Node.JS, add `"edge-aware"` flag so Typo4 would expect to see a callback function instead of a Node.JS app.
  - With Node.JS, add `"noinput"` flag to specify that this script only generates new text;
  - To understand easier how it might be useful and how it works, take a look at Scripts/3.Calculate.js. 
- Press Caps Lock+V to access the clipboard history;
  - Pin entries;
  - Quickly go back to them by pressing a corresponding button;
  - Exclude passwords via their checksums.
- Press Caps Lock+S to insert an Emoji or several:
  - Groups and names;
  - Switch between various sets and definitions in Typo4 settings;
  - Hold Ctrl to insert several Emojis at once.
- Press any other Caps Lock+… combination for character/text shortcuts in Bindings/Main.lua:
  - Bind any text to any key;
  - For instance, Caps Lock+X will get you “×”, quite convinient.
- No need to restart an app when any script is changed, it watches for changes in its Data directory;
- Any keystrokes starting with Caps Lock is being captured by Typo4;
- If you want to toggle Caps Lock status, use something like Shift+Caps Lock.

### Some details:

- It basically uses keyboard hooks, sends Ctrl+C/Ctrl+A/Ctrl+V keystrokes and modifies clipboard content to alter the text;
- Although simple and naive, this approach seems to work just fine, unless…
- UWP app is used. With those ones, usual copy-paste keystrokes not always work, and I have no idea why (and, frankly, 
  that’s the main reason I personally avoid using them in general, unrelated to Typo4);
- Why Typo4? There were three more iterations of that app, first one was made with AutoHotkey.

### License:

MIT.
