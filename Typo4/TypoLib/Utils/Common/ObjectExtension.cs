using System;
using System.Globalization;
using JetBrains.Annotations;

namespace TypoLib.Utils.Common {
    public static class ObjectExtension {
        [Pure]
        public static string ToInvariantString(this float o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString(this double o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }
        [Pure]
        public static string ToInvariantString(this int o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString(this uint o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString(this short o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString(this char o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString(this ushort o) {
            return o.ToString(CultureInfo.InvariantCulture);
        }

        [Pure]
        public static string ToInvariantString([NotNull] this object o) {
            if (o == null) throw new ArgumentNullException(nameof(o));
            switch (o) {
                case string r:
                    return r;
                case double d:
                    return d.ToInvariantString();
                case float f:
                    return f.ToInvariantString();
                case int i:
                    return i.ToInvariantString();
                case uint u:
                    return u.ToInvariantString();
                case short h:
                    return h.ToInvariantString();
                case ushort u:
                    return u.ToInvariantString();
                default:
                    return o.ToString();
            }
        }
    }
}