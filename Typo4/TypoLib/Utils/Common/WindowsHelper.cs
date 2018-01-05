using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace TypoLib.Utils.Common {
    public static class WindowsHelper {
        public const string RestartArg = "--restart";

        [Localizable(false), ContractAnnotation("=> halt")]
        public static void RestartCurrentApplication() {
            try {
                ProcessExtension.Start(MainExecutingFile.Location,
                        Environment.GetCommandLineArgs().Skip(1).ApartFrom(RestartArg)
                                   .Where(x => !x.StartsWith("acmanager:", StringComparison.OrdinalIgnoreCase)).Prepend(RestartArg));
                Environment.Exit(0);
            } catch (Exception e) {
                TypoLogging.Write(e);
            }
        }

        [Localizable(false)]
        public static void ViewDirectory([NotNull] string directory) {
            ProcessExtension.Start(@"explorer", new [] { directory });
        }

        [Localizable(false)]
        public static void ViewFile([NotNull] string filename) {
            Process.Start("explorer", "/select,\"" + filename + "\"");
        }

        [Localizable(false)]
        public static void ViewInBrowser([CanBeNull] string url) {
            if (string.IsNullOrWhiteSpace(url)) return;
            try {
                Process.Start(url);
            } catch (Exception) {
                TypoLogging.NonFatalErrorNotify("Can’t open link", $"App tried to open: “{url}”");
            }
        }

        [Localizable(false)]
        public static void ViewInBrowser([CanBeNull] Uri url) {
            ViewInBrowser(url?.AbsoluteUri);
        }

        [Localizable(false)]
        public static void OpenFile([NotNull] string filename) {
            Process.Start(filename);
        }

        [NotNull]
        public static IEnumerable<string> GetInputFiles(this DragEventArgs e) {
            return e.Data.GetData(DataFormats.FileDrop) as string[] ??
                    (e.Data.GetData(DataFormats.UnicodeText) as string)?.Split('\n')
                                                                        .Select(x => x.Trim())
                                                                        .Select(x => x.Length > 1 && x.StartsWith(@"""") && x.EndsWith(@"""")
                                                                                ? x.Substring(1, x.Length - 2) : x) ?? new string[0];
        }
    }
}
