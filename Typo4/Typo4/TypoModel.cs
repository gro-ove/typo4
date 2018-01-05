using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using Typo4.Clipboards;
using Typo4.Emojis;
using Typo4.Popups;
using Typo4.Utils;
using TypoLib;
using TypoLib.Utils;
using TypoLib.Utils.Common;

namespace Typo4 {
    /// <summary>
    /// Handles all logic in relationship to UI.
    /// </summary>
    public class TypoModel : NotifyPropertyChanged, IDisposable {
        #region Paths
        public static string DataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Typo4");
        public static string LogFilename => Path.Combine(DataDirectory, "Logs", "Typo Log.log");
        public static string DataFilename => Path.Combine(DataDirectory, "Values.data");

        static TypoModel() {
            if (!Directory.Exists(DataDirectory)) {
                Directory.CreateDirectory(DataDirectory);
            }
        }
        #endregion

        public static TypoModel Instance { get; private set; }

        private Typo _typo;
        public Typo Typo => _typo;

        private EmojisStorage _emojisStorage;
        private PasswordsContainer _passwordsContainer;
        private ClipboardHistory _clipboardHistory;
        private Autorun _autorun;

        [CanBeNull]
        public EmojisStorage EmojisStorage => _emojisStorage;

        [CanBeNull]
        public PasswordsContainer PasswordsContainer => _passwordsContainer;

        [CanBeNull]
        public ClipboardHistory ClipboardHistory => _clipboardHistory;

        [CanBeNull]
        public Autorun Autorun => _autorun;

        #region Common settings
        [CanBeNull]
        private RussianWindowsFix _russianWindowsFix;

        public bool RussianKeyboardFix {
            get => _russianWindowsFix != null;
            set {
                if (Equals(value, RussianKeyboardFix)) return;

                if (value) {
                    _russianWindowsFix = new RussianWindowsFix();
                } else {
                    DisposeHelper.Dispose(ref _russianWindowsFix);
                }

                ValuesStorage.Set("settings.russianKeyboardFix", value);
                OnPropertyChanged();
            }
        }

        private bool _closePopupsOnInsert = ValuesStorage.GetBool("settings.closePopupOnInsert", true);

        public bool ClosePopupsOnInsert {
            get => _closePopupsOnInsert;
            set {
                if (Equals(value, _closePopupsOnInsert)) return;
                _closePopupsOnInsert = value;
                OnPropertyChanged();
                ValuesStorage.Set("settings.closePopupOnInsert", value);
            }
        }
        #endregion

        public TypoModel() {
            Instance = this;
        }

        public void Initialize() {
            if (_typo != null) return;
            _typo = new Typo(DataDirectory);

            // App-related
            _autorun = new Autorun("Typo4", MainExecutingFile.Location);

            // List of emojis
            _emojisStorage = new EmojisStorage(DataDirectory);
            _typo.AddInserter(new EmojiPopup(_emojisStorage));

            // Clipboard history
            _passwordsContainer = new PasswordsContainer();
            _clipboardHistory = new ClipboardHistory(_passwordsContainer);
            _typo.AddInserter(new ClipboardPopup(_clipboardHistory));

            // Special fix for Russian Windows
            RussianKeyboardFix = ValuesStorage.GetBool("settings.russianKeyboardFix", true);
        }

        #region Commands
        private DelegateCommand _editTypoScriptCommand;

        public DelegateCommand EditTypoScriptCommand => _editTypoScriptCommand ?? (_editTypoScriptCommand = new DelegateCommand(() => {
            Process.Start(Path.Combine(DataDirectory, "TypoScript.lua"));
        }));

        private DelegateCommand _openDataDirectoryCommand;

        public DelegateCommand OpenDataDirectoryCommand => _openDataDirectoryCommand ?? (_openDataDirectoryCommand = new DelegateCommand(() => {
            Process.Start(DataDirectory);
        }));
        #endregion

        public void Dispose() {
            DisposeHelper.Dispose(ref _typo);
            DisposeHelper.Dispose(ref _russianWindowsFix);
        }
    }
}