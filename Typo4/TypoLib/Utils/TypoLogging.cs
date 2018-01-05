using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace TypoLib.Utils {
    /// <summary>
    /// Callback which will be called when non-fatal error happens about which user should be notified.
    /// </summary>
    public delegate void TypoLoggingWrite(string message, [CallerMemberName] string m = null, [CallerFilePath] string p = null, [CallerLineNumber] int l = -1);

    /// <summary>
    /// Callback which will be called when non-fatal error happens about which user should be notified.
    /// </summary>
    public delegate void TypoLoggingNonFatalErrorNotify(string message, string commentary, Exception exception);

    /// <summary>
    /// Abstract logging base.
    /// </summary>
    public static class TypoLogging {
        /// <summary>
        /// Callback which will be called to write a message to logs.
        /// </summary>
        public static TypoLoggingWrite Logger;

        /// <summary>
        /// Callback which will be called when non-fatal error happens about which user should be notified.
        /// </summary>
        public static TypoLoggingNonFatalErrorNotify TypoLoggingNonFatalErrorHandler;

        public static void Write(object s, [CallerMemberName] string m = null, [CallerFilePath] string p = null, [CallerLineNumber] int l = -1) {
            if (Logger == null) {
                Console.WriteLine(s?.ToString() ?? "<NULL>");
            } else {
                Logger.Invoke(s?.ToString() ?? "<NULL>", m, p, l);
            }
        }

        /// <summary>
        /// Notify about some non-fatal exception. User will see some message.
        /// </summary>
        /// <param name="message">Ex.: “Can’t do this and that”.</param>
        /// <param name="commentary">Ex.: “Make sure A is something and B is something else.”</param>
        /// <param name="exception">Exception which caused the problem.</param>
        public static void NonFatalErrorNotify([NotNull] string message, [CanBeNull] string commentary, Exception exception = null) {
            if (TypoLoggingNonFatalErrorHandler == null) {
                Console.WriteLine("Non-fatal error: " + message);

                if (commentary != null) {
                    Console.WriteLine(commentary);
                }

                if (exception != null) {
                    Console.WriteLine(exception);
                }
            } else {
                ReportExceptionWithHandler(message, commentary, exception);
            }
        }

        private static void ReportExceptionWithHandler(string message, string commentary, Exception exception) {
            while (true) {
                switch (exception) {
                    case AggregateException aggregateException when aggregateException.InnerException != null && aggregateException.InnerExceptions.Count == 1:
                        exception = aggregateException.InnerException;
                        continue;

                    case ScriptRuntimeException luaRuntimeException:
                        TypoLoggingNonFatalErrorHandler.Invoke("Lua runtime exception: " + luaRuntimeException.DecoratedMessage, commentary, exception);
                        return;

                    case SyntaxErrorException luaSyntaxErrorException:
                        TypoLoggingNonFatalErrorHandler.Invoke("Lua syntax exception: " + luaSyntaxErrorException.DecoratedMessage, commentary, exception);
                        return;

                    case InterpreterException luaException:
                        TypoLoggingNonFatalErrorHandler.Invoke("Lua exception: " + luaException.DecoratedMessage, commentary, exception);
                        return;

                    default:
                        TypoLoggingNonFatalErrorHandler.Invoke(message, commentary, exception);
                        return;
                }
            }
        }
    }
}