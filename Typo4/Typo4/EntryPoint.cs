﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Windows.Controls;
using TypoLib.Utils;
using TypoLib.Utils.Common;
using TypoLib.Utils.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Typo4 {
    public static class EntryPoint {
        private static bool _initialized;

        [STAThread]
        private static void Main(string[] a) {
            try {
                MainReal(a);
            } catch (Exception e) {
                UnhandledExceptionHandler(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void MainReal(string[] a) {
            if (!Debugger.IsAttached) {
                SetUnhandledExceptionHandler();
            }

            MainInner(a);
        }

        public static uint SecondInstanceMessage { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MainInner(string[] args) {
            // if (AppUpdater.OnStartup(args)) return;

            if (args.Length == 2 && args[0] == "--run") {
                Process.Start(args[1]);
                return;
            }

            var mainAssembly = Assembly.GetEntryAssembly();
            if (mainAssembly == null) {
                return;
            }

            var appGuid = ((GuidAttribute)mainAssembly.GetCustomAttributes(typeof(GuidAttribute), true).GetValue(0)).Value;
            var mutexId = $@"Global\{{{appGuid}}}";

            if (args.Contains(WindowsHelper.RestartArg)) {
                for (var i = 0; i < 999; i++) {
                    Thread.Sleep(200);

                    using (var mutex = new Mutex(false, mutexId)) {
                        if (mutex.WaitOne(0, false)) {
                            break;
                        }
                    }
                }

                ProcessExtension.Start(MainExecutingFile.Location, args.Where(x => x != WindowsHelper.RestartArg));
                return;
            }

            using (var mutex = new Mutex(false, mutexId)) {
                SecondInstanceMessage = User32.RegisterWindowMessage(mutexId);
                if (mutex.WaitOne(0, false)) {
                    _initialized = true;
                    App.CreateAndRun();
                } else {
                    PassArgsToRunningInstance(args);
                }
            }
        }

        private static string CommunicationFilename => Path.Combine(Path.GetTempPath(), "__cm_cuymbo6r");

        [Localizable(false)]
        private static string PackArgs(IEnumerable<string> args) {
            return args.Select(x => "\n" + x.Replace(@"\", @"\\").Replace("\n", @"\n")).JoinToString();
        }

        [Localizable(false)]
        private static IEnumerable<string> UnpackArgs(string packed) {
            return packed.Split('\n').Select(x => x.Replace(@"\n", "\n").Replace(@"\\", @"\")).Skip(1);
        }

        public static void PassSomeData(IEnumerable<string> data) {
            try {
                File.WriteAllText(CommunicationFilename, PackArgs(data));
            } catch (Exception) {
                // ignored
            }
        }

        private static IEnumerable<string> ReceiveSomeData() {
            if (!File.Exists(CommunicationFilename)) return new string[] { };

            try {
                var text = File.ReadAllText(CommunicationFilename);
                File.Delete(CommunicationFilename);
                return UnpackArgs(text);
            } catch (Exception e) {
                Logging.Warning("Cannot receive data: " + e);
                return new string[] { };
            }
        }

        private static void PassArgsToRunningInstance(IEnumerable<string> args) {
            PassSomeData(args);
            User32.PostMessage(User32.HWND_BROADCAST, SecondInstanceMessage, 0, 0);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void SetUnhandledExceptionHandler() {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

#if DEBUG
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
#endif
        }

        private static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args) {
            var e = args.Exception as Exception;
            var app = System.Windows.Application.Current;
            if (app != null) {
                app.Dispatcher.Invoke(() => {
                    UnhandledExceptionHandler(e);
                });
            } else {
                UnhandledExceptionHandler(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoggingIsAvailable() {
            return Logging.IsInitialized();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LogError(string text) {
            try {
                if (!LoggingIsAvailable()) return false;
                Logging.Error(text);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnhandledExceptionFancyHandler(Exception e) {
            if (System.Windows.Application.Current?.Windows.OfType<Window>().Any(x => x.IsLoaded && x.IsVisible) != true) {
                throw new Exception();
            }

            DpiAwareWindow.OnFatalError(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnhandledExceptionHandler(Exception e) {
            if (e is InvalidOperationException && e.Message.Contains("Visibility") && e.Message.Contains("WindowInteropHelper.EnsureHandle")) {
                Logging.Error(e.ToString());
                return;
            }

            var text = e?.ToString() ?? @"?";

            if (!_initialized) {
                try {
                    MessageBox.Show(text, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } catch (Exception) {
                    // ignored
                }
                Environment.Exit(1);
            }

            if (!LogError(text)) {
                try {
                    var logFilename = $"{AppDomain.CurrentDomain.BaseDirectory}/content_manager_crash_{DateTime.Now.Ticks}.txt";
                    File.WriteAllText(logFilename, text);
                } catch (Exception) {
                    // ignored
                }
            }

            try {
                UnhandledExceptionFancyHandler(e);
                return;
            } catch (Exception ex) {
                LogError(ex.Message);

                try {
                    MessageBox.Show(text, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } catch (Exception) {
                    // ignored
                }
            }

            Environment.Exit(1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnhandledExceptionHandler_Initial(object sender, UnhandledExceptionEventArgs args) {
            Logging.Error(args?.ExceptionObject);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
            try {
                UnhandledExceptionHandler_Initial(sender, args);
            } catch {
                // ignored
            }

            var e = args.ExceptionObject as Exception;
            var app = System.Windows.Application.Current;
            if (app != null) {
                app.Dispatcher.Invoke(() => {
                    UnhandledExceptionHandler(e);
                });
            } else {
                UnhandledExceptionHandler(e);
            }
        }

        public static void HandleSecondInstanceMessages(Window window, Action<IEnumerable<string>> handler) {
            HwndSource hwnd = null;
            HwndSourceHook hook = (IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled) => {
                if (message == SecondInstanceMessage) {
                    try {
                        handler(ReceiveSomeData());
                        (window as DpiAwareWindow)?.BringToFront();
                    } catch (Exception e) {
                        Logging.Warning("Can’t handle message: " + e);
                    }
                }

                return IntPtr.Zero;
            };

            RoutedEventHandler[] handlers = { null, null };

            // loaded
            handlers[0] = (sender, args) => {
                window.Loaded -= handlers[0];

                try {
                    hwnd = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
                    if (hwnd == null) {
                        Logging.Warning("Can’t add one-instance hook: HwndSource is null");
                        return;
                    }

                    hwnd.AddHook(hook);
                    window.Unloaded += handlers[1];
                } catch (Exception e) {
                    Logging.Warning("Can’t add one-instance hook: " + e);
                    hook = null;
                }
            };

            // unloaded
            handlers[1] = (sender, args) => {
                window.Unloaded -= handlers[1];

                try {
                    hwnd.RemoveHook(hook);
                } catch (Exception e) {
                    Logging.Warning("Can’t remove one-instance hook: " + e);
                    hook = null;
                }
            };

            if (!window.IsLoaded) {
                window.Loaded += handlers[0];
            } else {
                handlers[0](null, null);
            }
        }
    }
}
