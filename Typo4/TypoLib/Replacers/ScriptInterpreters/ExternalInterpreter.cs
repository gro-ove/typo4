using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TypoLib.Utils;
using TypoLib.Utils.Common;

namespace TypoLib.Replacers.ScriptInterpreters {
    public class ExternalInterpreterException : Exception {
        public readonly string Cmd, Stdout, Stderr;
        public readonly int Code;

        public ExternalInterpreterException(string cmd, int code, string stdout, string stderr)
                : base("Can’t execute: “" + cmd + "”") {
            Cmd = cmd;
            Code = code;
            Stdout = stdout;
            Stderr = stderr;
        }
    }

    public class ExternalInterpreter : IScriptInterpreter {
        [CanBeNull]
        private readonly string _executableName;

        [NotNull]
        private readonly string[] _arguments;

        public ExternalInterpreter([CanBeNull] string executableName, [NotNull] params string[] arguments) {
            _executableName = executableName;
            _arguments = arguments;
        }

        [CanBeNull]
        private string _workingDirectory;

        public void Initialize(string scripsDirectory) {
            _workingDirectory = scripsDirectory;
        }

        private string Execute(string filename, string originalText) {
            using (var p = ProcessExtension.Start(
                    _executableName ?? filename,
                    _executableName == null ? null : _arguments.Append(filename),
                    new ProcessStartInfo {
                        WorkingDirectory = _workingDirectory ?? ".",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    })) {
                p.Start();

                using (var writer = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8)) {
                    writer.Write(originalText);
                    writer.Close();
                }

                p.WaitForExit();

                var output = p.StandardOutput.ReadToEnd().Trim();
                var error = p.StandardError.ReadToEnd().Trim();

                if (p.ExitCode != 0) {
                    var sb = new StringBuilder();
                    sb.AppendLine("Exit code: " + p.ExitCode);

                    if (output.Length > 0) {
                        sb.Append("\n\n");
                        sb.AppendLine(output);
                    }

                    if (error.Length > 0) {
                        sb.Append("\n\n");
                        sb.AppendLine(error);
                    }

                    throw new ExternalInterpreterException(_executableName + " " + _arguments, p.ExitCode, output, error);
                }

                p.Close();
                return output;
            }
        }

        public Task<string> ExecuteAsync(string filename, string originalText, CancellationToken cancellation) {
            return Task.Run(() => Execute(filename, originalText));
        }

        public bool IsInputSupported(string filename) {
            return true;
        }

        public void Dispose() { }
    }
}