using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Typo4.Utils;
using Typo4.Windows;
using TypoLib.Utils.Common;

namespace Typo4 {
    public class TrayInterface : IDisposable {
        private readonly TypoModel _model;
        private TaskbarIcon _icon;

        public TrayInterface(TypoModel model) {
            _model = model;
            _icon = new TaskbarIcon {
                Icon = AppIconService.GetTrayIcon(),
                ToolTipText = "Typo4 is running",
                ContextMenu = new ContextMenu {
                    Items = {
                        new MenuItem { Header = "Open data directory", Command = _model.OpenDataDirectoryCommand },
                        new MenuItem { Header = "Run Typo4 when my computer starts", IsCheckable = true }.AddBinding(MenuItem.IsCheckedProperty,
                                new Binding { Path = new PropertyPath(nameof(_model.Autorun.IsActive)), Source = _model.Autorun, Mode = BindingMode.TwoWay }),
                        new MenuItem { Header = "Settings", Command = new DelegateCommand(WakeUp) },
#if DEBUG
                        new Separator(),
                        new MenuItem { Header = "Debug window", IsCheckable = true }.AddBinding(MenuItem.IsCheckedProperty,
                                new Binding { Path = new PropertyPath(nameof(model.Typo.IsDebugFormActive)), Source = model.Typo, Mode = BindingMode.TwoWay }),
#endif
                        new Separator(),
                        new MenuItem { Header = "Exit", Command = new DelegateCommand(Exit) }
                    }
                },
                DoubleClickCommand = new DelegateCommand(WakeUp)
            };
        }

        private void Exit() {
            new Thread(() => {
                Thread.Sleep(300);
                Environment.Exit(0);
            }).Start();
            Application.Current?.Shutdown();
        }

        private MainWindow _mainWindow;

        private void WakeUp() {
            if (_mainWindow != null && _mainWindow.IsVisible) {
                _mainWindow.Activate();
            } else {
                (_mainWindow = new MainWindow()).ShowAndWaitAsync().ContinueWith(t => _mainWindow = null);
            }
        }

        public void Dispose() {
            DisposeHelper.Dispose(ref _icon);
        }
    }
}