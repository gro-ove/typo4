﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TypoLib.Utils.Windows;

namespace TypoLib.Utils.Common {
    public static class ProcessExtension {
        public static string GetQuotedArgument([CanBeNull] string argument) {
            if (string.IsNullOrEmpty(argument)) return "";

            // The argument is processed in reverse character order.
            // Any quotes (except the outer quotes) are escaped with backslash.
            // Any sequences of backslashes preceding a quote (including outer quotes) are doubled in length.
            var resultBuilder = new StringBuilder();

            var outerQuotesRequired = HasWhitespace(argument);

            var precedingQuote = false;
            if (outerQuotesRequired) {
                resultBuilder.Append('"');
                precedingQuote = true;
            }

            for (var index = argument.Length - 1; index >= 0; index--) {
                var @char = argument[index];
                resultBuilder.Append(@char);

                if (@char == '"') {
                    precedingQuote = true;
                    resultBuilder.Append('\\');
                } else if (@char == '\\' && precedingQuote) {
                    resultBuilder.Append('\\');
                } else {
                    precedingQuote = false;
                }
            }

            if (outerQuotesRequired) {
                resultBuilder.Append('"');
            }

            return Reverse(resultBuilder.ToString());
        }

        private static bool HasWhitespace([NotNull] string text) {
            return text.Any(char.IsWhiteSpace);
        }

        private static string Reverse([NotNull] string text) {
            return new string(text.Reverse().ToArray());
        }

        public static Process Start([NotNull] string filename, [CanBeNull] IEnumerable<string> args, bool shell = true) {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            return Process.Start(new ProcessStartInfo {
                FileName = filename,
                Arguments = args?.Select(GetQuotedArgument).JoinToString(" ") ?? "",
                UseShellExecute = shell
            });
        }

        public static Process Start([NotNull] string filename, [CanBeNull] IEnumerable<string> args, ProcessStartInfo startInfo) {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            startInfo.FileName = filename;
            startInfo.Arguments = args?.Select(GetQuotedArgument).JoinToString(" ") ?? "";
            return Process.Start(startInfo);
        }

        public static bool HasExitedSafe([NotNull] this Process process) {
            if (process == null) throw new ArgumentNullException(nameof(process));
            var handle = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryLimitedInformation | Kernel32.ProcessAccessFlags.Synchronize, false, process.Id);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1)) return true;

            try {
                if (Kernel32.GetExitCodeProcess(handle, out var exitCode) && exitCode != Kernel32.STILL_ACTIVE) return true;
                using (var w = new ProcessWrapper.ProcessWaitHandle(handle)) {
                    return w.WaitOne(0, false);
                }
            } finally {
                Kernel32.CloseHandle(handle);
            }
        }

        private static async Task WaitForExitAsyncDeeperFallback([NotNull] Process process, CancellationToken cancellationToken = default(CancellationToken)) {
            if (process == null) throw new ArgumentNullException(nameof(process));

            TypoLogging.Write("Is there an issue?");

            var processId = process.Id;
            while (true) {
                await Task.Delay(300, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;

                try {
                    Process.GetProcessById(processId);
                } catch (ArgumentException) {
                    return;
                }
            }
        }

        private static async Task WaitForExitAsyncFallback([NotNull] Process process, CancellationToken cancellationToken = default(CancellationToken)) {
            if (process == null) throw new ArgumentNullException(nameof(process));

            var handle = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryLimitedInformation | Kernel32.ProcessAccessFlags.Synchronize, false, process.Id);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1)) {
                await WaitForExitAsyncDeeperFallback(process, cancellationToken);
                return;
            }

            try {
                if (Kernel32.GetExitCodeProcess(handle, out var exitCode) && exitCode != Kernel32.STILL_ACTIVE) return;
                using (var w = new ProcessWrapper.ProcessWaitHandle(handle)) {
                    TypoLogging.Write("Waiting using ProcessWaitHandle…");

                    while (!w.WaitOne(0, false)) {
                        await Task.Delay(300, cancellationToken);
                        if (cancellationToken.IsCancellationRequested) return;
                    }
                }
            } finally {
                Kernel32.CloseHandle(handle);
            }
        }

        public static Task WaitForExitAsync([NotNull] this Process process, CancellationToken cancellationToken = default(CancellationToken)) {
            if (process == null) throw new ArgumentNullException(nameof(process));
            try {
                var tcs = new TaskCompletionSource<object>();
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) => tcs.TrySetResult(null);
                if (cancellationToken != default(CancellationToken)) {
                    cancellationToken.Register(() => { tcs.TrySetCanceled(); });
                }

                return tcs.Task;
            } catch (Exception e) {
                TypoLogging.Write(e);
                return WaitForExitAsyncFallback(process, cancellationToken);
            }
        }

        /// <summary>
        /// Might be very slow (up to ≈700ms) if GetProcessPathUsingPsApi won’t work properly.
        /// Returns null when all three ways failed.
        /// </summary>
        /// <param name="process">Process.</param>
        /// <returns>Path to process’s executable file.</returns>
        [CanBeNull]
        public static string GetFilenameSafe([NotNull] this Process process) {
            if (process == null) throw new ArgumentNullException(nameof(process));
            try {
                var path = GetProcessPathUsingPsApi(process.Id);
                if (path != null) {
                    TypoLogging.Write("PS API: " + path);
                    return path;
                }

                // very slow
                /*path = GetProcessPathUsingManagement(process.Id);
                if (path != null) {
                    TypoLogging.Write("Management: " + path);
                    return path;
                }

                TypoLogging.Write("Management failed!");*/

                // won’t work if processes were compiled for different architectures
                path = process.MainModule.FileName;
                TypoLogging.Write("MainModule.FileName: " + path);
                return path;
            } catch (Exception e) {
                TypoLogging.Write(e);
                return null;
            }
        }

        [DllImport(@"psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In, MarshalAs(UnmanagedType.U4)]  int nSize);

        private static string GetProcessPathUsingPsApi(int pid) {
            var processHandle = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryInformation, false, pid);
            if (processHandle == IntPtr.Zero) return null;

            const int lengthSb = 4000;

            try {
                var sb = new StringBuilder(lengthSb);
                return GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0 ? sb.ToString() : null;
            } catch (Exception e) {
                TypoLogging.Write(e);
                return null;
            } finally {
                Kernel32.CloseHandle(processHandle);
            }
        }

        /*[CanBeNull]
        private static string GetProcessPathUsingManagement(int processId) {
            try {
                using (var s = new ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}"))
                using (var c = s.Get()) {
                    return c.Cast<ManagementObject>().Select(x => x[@"ExecutablePath"]).FirstOrDefault()?.ToString();
                }
            } catch (Exception e) {
                TypoLogging.Write(e);
            }

            return null;
        }*/

        public static IReadOnlyList<IntPtr> GetWindowsHandles([NotNull] this Process process) {
            if (process == null) throw new ArgumentNullException(nameof(process));
            var handles = new List<IntPtr>();
            foreach (ProcessThread thread in Process.GetProcessById(process.Id).Threads) {
                User32.EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }
            return handles;
        }
    }
}
