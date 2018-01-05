using System;
using System.Windows.Threading;
using FirstFloor.ModernUI.Presentation;

namespace Typo4.Utils {
    public static class MuiSystemAccent {
        private static DispatcherTimer _systemAccentCheckTimer;

        public static void Initialize() {
            AppearanceManager.Current.AccentColor = AccentColorSet.ActiveSet["SystemAccent"];

            // Periodically check if system accent color is changed
            _systemAccentCheckTimer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(5),
                IsEnabled = true
            };

            _systemAccentCheckTimer.Tick += OnSystemAccentCheckTimerTick;
        }

        public static event EventHandler SystemColorsChanged;

        private static void OnSystemAccentCheckTimerTick(object sender, EventArgs eventArgs) {
            if (AccentColorSet.ActiveSet.IsColorUpdated("SystemAccent")) {
                AppearanceManager.Current.AccentColor = AccentColorSet.ActiveSet["SystemAccent"];
                SystemColorsChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}