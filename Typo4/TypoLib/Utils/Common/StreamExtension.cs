﻿using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TypoLib.Utils.Common {
    public static class StreamExtension {
        [NotNull]
        public static byte[] ReadAsBytes([NotNull] this Stream s) {
            if (s == null) throw new ArgumentNullException(nameof(s));
            using (var m = new MemoryStream()) {
                s.CopyTo(m);
                return m.ToArray();
            }
        }

        [NotNull]
        public static byte[] ReadAsBytesAndDispose([NotNull] this Stream s) {
            using (s) {
                return ReadAsBytes(s);
            }
        }

        [NotNull]
        public static async Task<byte[]> ReadAsBytesAsync([NotNull] this Stream s) {
            if (s == null) throw new ArgumentNullException(nameof(s));
            using (var m = new MemoryStream()) {
                await s.CopyToAsync(m);
                return m.ToArray();
            }
        }

        [NotNull]
        public static async Task<byte[]> ReadAsBytesAndDisposeAsync([NotNull] this Stream s) {
            using (s) {
                return await ReadAsBytesAsync(s);
            }
        }

        [NotNull]
        public static void WriteBytes([NotNull] this Stream s, byte[] data) {
            if (s == null) throw new ArgumentNullException(nameof(s));
            s.Write(data, 0, data.Length);
        }

        [NotNull]
        public static void WriteBytesAndDispose([NotNull] this Stream s, byte[] data) {
            using (s) {
                WriteBytes(s, data);
            }
        }

        [NotNull]
        public static MemoryStream ReadAsMemoryStream([NotNull] this Stream s) {
            var m = new MemoryStream();
            s.CopyTo(m);
            return m;
        }

        [NotNull]
        public static MemoryStream ReadAsMemoryStreamAndDispose([NotNull] this Stream s) {
            using (s) {
                return ReadAsMemoryStream(s);
            }
        }

        [NotNull]
        public static string ReadAsString([NotNull] this Stream s) {
            return ReadAsBytes(s).ToUtf8String();
        }

        /// <summary>
        /// Using UTF8 (only if it’s a correct one) or Default encoding
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public static string ReadAsStringAndDispose([NotNull] this Stream s) {
            return ReadAsBytesAndDispose(s).ToUtf8String();
        }

        public static void CopyTo(this Stream input, Stream output, int bytes, int bufferSize = 81920) {
            var buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        public static async Task CopyToAsync(this Stream input, Stream output, long bytes, int bufferSize = 81920) {
            var buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 && (read = await input.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytes))) > 0) {
                await output.WriteAsync(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
