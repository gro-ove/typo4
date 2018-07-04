using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Win32;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Typo4.Utils;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using Logging = FirstFloor.ModernUI.Helpers.Logging;

namespace Typo4 {
    public partial class App : IAppIconProvider, FatalErrorMessage.IAppRestartHelper, IEmojiProvider {
        public static void CreateAndRun() {
            new App().Run();
        }

        public App() {
            // FirstFloor.ModernUI initialization (library used here is a forked version from Content Manager project with extra bits)
            ValuesStorage.Initialize(TypoModel.DataFilename, "_key_zsu4b3ws1k17ties_" + Environment.UserName, true);
            CacheStorage.Initialize();
            Logging.Initialize(TypoModel.LogFilename, false);
            Logging.Write($"App version: {BuildInformation.AppVersion} ({BuildInformation.Platform}, {WindowsVersionHelper.GetVersion()})");

            if (Directory.GetFiles(TypoModel.DataDirectory).Length == 0) {
                using (var memory = new MemoryStream(Typo4.Properties.Resources.Typo4Data)) {
                    new ZipArchive(memory).ExtractToDirectory(TypoModel.DataDirectory);
                }
            }

            AppearanceManager.Current.Initialize();
            AppearanceManager.Current.SetTheme(new Uri("pack://application:,,,/Typo4;component/Assets/AppTheme.xaml", UriKind.Absolute));
            AppearanceManager.Current.BoldTitleLinks = false;
            AppearanceManager.Current.LargerTitleLinks = false;
            AppearanceManager.Current.SubMenuFontSize = FontSize.Small;
            Resources.MergedDictionaries.Add(new ResourceDictionary{
                Source = new Uri("pack://application:,,,/Typo4;component/Assets/AppAssets.xaml", UriKind.Absolute)
            });

            MuiSystemAccent.Initialize();
            NonfatalError.Initialize();
            AppIconService.Initialize(this);
            FatalErrorMessage.Register(this);

            // All logic in relationship to UI
            _typoModel = new TypoModel();

            // Some more UI initialization bits
            PrepareUi();
            AppShortcut.Initialize("x4fab.Typo4", "Typo4");
            Toast.SetDefaultAction(() => (Current.Windows.OfType<ModernWindow>().FirstOrDefault(x => x.IsActive) ??
                    Current.MainWindow as ModernWindow)?.BringToFront());
            BbCodeBlock.OptionEmojiProvider = this;

            // Let’s at least try to handle stuff properly
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            // Log stuff using FirstFloor.ModernUI library
            TypoLogging.Logger = (s, m, p, l) => Logging.Write($"{s} (Typo)", m, p, l);
            TypoLogging.TypoLoggingNonFatalErrorHandler = (s, c, e) => NonfatalError.Notify(s, c, e);

            // Most of the time app should work from system tray
            _typoModel.Initialize();
            _trayInterface = new TrayInterface(_typoModel);

            // Close only manually
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Fancy blur for all windows
            DpiAwareWindow.NewWindowCreated += OnWindowLoaded;
        }

        private static void OnWindowLoaded(object sender, EventArgs e) {
            if (sender is ModernDialog w && !w.IsLoaded) {
                w.WindowStyle = WindowStyle.None;
                w.AllowsTransparency = true;
                w.Topmost = true;
                w.ShowInTaskbar = false;
                w.BlurBackground = true;
                w.PreventActivation = true;

                var backgroundColor = (Color)AppearanceManager.Current.CurrentThemeDictionary["WindowBackgroundColor"];
                backgroundColor.A = 230;
                w.Background = new SolidColorBrush(backgroundColor);
            }
        }

        private static void PrepareUi() {
            try {
                ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(true));
                ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(500));
                ToolTipService.BetweenShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(500));
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(60000));
                ItemsControl.IsTextSearchCaseSensitiveProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(true));
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(60));
                RenderOptions.BitmapScalingModeProperty.OverrideMetadata(typeof(BetterImage), new FrameworkPropertyMetadata(BitmapScalingMode.HighQuality));
            } catch (Exception e) {
                Logging.Error(e);
            }

            PopupHelper.Initialize();
        }

        private TypoModel _typoModel;
        private TrayInterface _trayInterface;

        private void OnProcessExit(object sender, EventArgs e) {
            DisposeHelper.Dispose(ref _trayInterface);
            DisposeHelper.Dispose(ref _typoModel);
        }

        Uri IAppIconProvider.GetTrayIcon() {
            return WindowsVersionHelper.IsWindows10OrGreater ?
                    new Uri("pack://application:,,,/Typo4;component/Assets/Icons/TrayIcon.ico", UriKind.Absolute) :
                    new Uri("pack://application:,,,/Typo4;component/Assets/Icons/TrayIconWin8.ico", UriKind.Absolute);
        }

        Uri IAppIconProvider.GetAppIcon() {
            return new Uri("pack://application:,,,/Typo4;component/Assets/Icons/AppIcon.ico", UriKind.Absolute);
        }

        void FatalErrorMessage.IAppRestartHelper.Restart() {
            WindowsHelper.RestartCurrentApplication();
        }

        Uri IEmojiProvider.GetUri(string emojiCode) {
            return null;
        }

        ImageSource IEmojiProvider.GetImageSource(string emojiCode) {
            return _typoModel.EmojisStorage?.GetImageSource(emojiCode);
        }
    }
}
