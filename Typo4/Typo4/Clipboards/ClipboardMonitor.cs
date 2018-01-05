// https://stackoverflow.com/a/29919287/4267982

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TypoLib.Utils.Windows;

namespace Typo4.Clipboards {
    public static class ClipboardMonitor {
        public static event EventHandler ClipboardChange {
            add {
                ClipboardWatcher.StartListening();
                ClipboardWatcher.ClipboardChangeInner += value;
            }
            remove {
                ClipboardWatcher.ClipboardChangeInner -= value;
                if (!ClipboardWatcher.HasAnyListeners) {
                    ClipboardWatcher.StopListening();
                }
            }
        }

        private class ClipboardWatcher : Form {
            private static ClipboardWatcher _instance;
            private static IntPtr _nextClipboardViewer;
            public static event EventHandler ClipboardChangeInner;
            public static bool HasAnyListeners => ClipboardChangeInner != null;

            public static void StartListening() {
                if (_instance != null) return;
                var t = new Thread(x => Application.Run(new ClipboardWatcher()));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }

            public static void StopListening() {
                _instance.Invoke(new MethodInvoker(() => ChangeClipboardChain(_instance.Handle, _nextClipboardViewer)));
                _instance.Invoke(new MethodInvoker(_instance.Close));
                _instance.Dispose();
                _instance = null;
            }

            protected override void SetVisibleCore(bool value) {
                CreateHandle();
                _instance = this;
                _nextClipboardViewer = SetClipboardViewer(_instance.Handle);
                base.SetVisibleCore(false);
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

            private const int WmDrawClipboard = 0x308;
            private const int WmChangeCbChain = 0x030D;

            protected override void WndProc(ref Message m) {
                switch (m.Msg) {
                    case WmDrawClipboard:
                        ClipboardChangeInner?.Invoke(null, EventArgs.Empty);
                        User32.SendMessage(_nextClipboardViewer, (uint)m.Msg, m.WParam, m.LParam);
                        break;

                    case WmChangeCbChain:
                        if (m.WParam == _nextClipboardViewer) {
                            _nextClipboardViewer = m.LParam;
                        } else {
                            User32.SendMessage(_nextClipboardViewer, (uint)m.Msg, m.WParam, m.LParam);
                        }
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }
    }
}