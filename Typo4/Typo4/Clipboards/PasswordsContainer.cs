using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TypoLib.Utils.Common;

namespace Typo4.Clipboards {
    /// <summary>
    /// Stores user passwords as checksums allowing to check later if a piece of text is in fact
    /// that password or, optionally, contains it inside (within length limitation). Passwords
    /// should be longer than four symbols, but shorter than 100.
    /// </summary>
    /// <remarks>
    /// Uses SHA-256 for checksums and unique random 64-bit salts. As far as I understand,
    /// all that should make it pretty difficult to crack.
    /// </remarks>
    public class PasswordsContainer : NotifyPropertyChanged, IDisposable {
        public class StoredPassword {
            [JsonProperty("salt")]
            public string Salt { get; }

            [JsonProperty("checksum")]
            public string Checksum { get; }

            [JsonConstructor]
            public StoredPassword(string salt, string checksum) {
                Salt = salt;
                Checksum = checksum;
            }
        }

        #region Initialization
        [NotNull]
        private readonly HashAlgorithm _hashAlgorithm;

        [NotNull]
        private readonly List<StoredPassword> _userPasswords;

        public PasswordsContainer() {
            _hashAlgorithm = SHA256.Create();
            _userPasswords = LoadList();
        }

        public void Dispose() {
            _hashAlgorithm.Dispose();
        }
        #endregion

        #region Loading and saving
        private const string KeyPasswords = "userPasswords.list";

        private static List<StoredPassword> LoadList() {
            try {
                if (ValuesStorage.Contains(KeyPasswords)) {
                    return JsonConvert.DeserializeObject<List<StoredPassword>>(ValuesStorage.GetEncryptedString(KeyPasswords));
                }
            } catch (Exception e) {
                Logging.Warning(e);
            }

            return new List<StoredPassword>();
        }

        private static void SaveList(List<StoredPassword> passwords) {
            ValuesStorage.SetEncrypted(KeyPasswords, JsonConvert.SerializeObject(passwords));
        }
        #endregion

        #region Private methods
        private StoredPassword GetChecksum(string value) {
            var salt = StringExtension.RandomString(8);
            return new StoredPassword(salt, _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(salt + value)).ToHexString());
        }

        private string GetChecksum(string salt, string value) {
            return _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(salt + value)).ToHexString();
        }

        private bool ContainsPassword(string value) {
            for (var i = _userPasswords.Count - 1; i >= 0; i--) {
                var v = _userPasswords[i];
                if (GetChecksum(v.Salt, value) == v.Checksum) {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Parameters related to substrings
        private bool _checkSubstrings = ValuesStorage.GetBool("userPasswords.checkSubstrings");

        public bool CheckSubstrings {
            get => _checkSubstrings;
            set {
                if (Equals(value, _checkSubstrings)) return;
                _checkSubstrings = value;
                OnPropertyChanged();
                ValuesStorage.Set("userPasswords.checkSubstrings", value);
            }
        }

        private int _maxLengthToCheckSubstrings = ValuesStorage.GetInt("userPasswords.maxLengthToCheckSubstrings", 20);

        public int MaxLengthToCheckSubstrings {
            get => _maxLengthToCheckSubstrings;
            set {
                if (Equals(value, _maxLengthToCheckSubstrings)) return;
                _maxLengthToCheckSubstrings = value;
                OnPropertyChanged();
                ValuesStorage.Set("userPasswords.maxLengthToCheckSubstrings", value);
            }
        }
        #endregion

        #region Public methods
        public int Count => _userPasswords.Count;

        private DelegateCommand _clearUserPasswordsCommand;

        public DelegateCommand ClearUserPasswordsCommand => _clearUserPasswordsCommand ?? (_clearUserPasswordsCommand = new DelegateCommand(() => {
            _userPasswords.Clear();
            SaveList(_userPasswords);
            OnPropertyChanged(nameof(Count));
        }));

        public void AddPassword(string value) {
            value = value.Trim();

            // Do not check for substrings here
            if (ContainsPassword(value)) return;

            _userPasswords.Add(GetChecksum(value));
            SaveList(_userPasswords);
            OnPropertyChanged(nameof(Count));
        }

        public bool IsPassword(string value) {
            value = value.Trim();
            if (value.Length > 100) return false;
            if (ContainsPassword(value)) return true;
            if (CheckSubstrings && value.Length <= MaxLengthToCheckSubstrings) {
                for (var length = 4; length < value.Length; length++) {
                    for (var start = 0; start <= value.Length - length; start++) {
                        if (ContainsPassword(value.Substring(start, length))) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}