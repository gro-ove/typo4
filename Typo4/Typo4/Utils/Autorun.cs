using FirstFloor.ModernUI.Presentation;
using Microsoft.Win32;

namespace Typo4.Utils {
    public class Autorun : NotifyPropertyChanged {
        private readonly string _appName;
        private readonly string _executablePath;

        public Autorun(string appName, string executablePath) {
            _appName = appName;
            _executablePath = executablePath;
        }

        private static RegistryKey GetRegistryKey() {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        }

        private bool? _isActive;

        public bool IsActive {
            get => _isActive ?? (_isActive = GetRegistryKey().GetValue(_appName) != null).Value;
            set {
                if (Equals(value, IsActive)) return;
                _isActive = value;

                if (value){
                    GetRegistryKey().SetValue(_appName, _executablePath);
                } else {
                    GetRegistryKey().DeleteValue(_appName, false);
                }

                OnPropertyChanged();
            }
        }
    }

}