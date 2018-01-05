using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace Typo4.Utils {
    public static class FrameworkElementExtension {
        public static T AddBinding<T>([NotNull] this T obj, DependencyProperty dp, BindingBase binding) where T : FrameworkElement {
            obj.SetBinding(dp, binding);
            return obj;
        }

        public static T AddBinding<T>([NotNull] this T obj, DependencyProperty dp, string binding) where T : FrameworkElement {
            obj.SetBinding(dp, binding);
            return obj;
        }

        public static T Seal<T>([NotNull] this T obj) where T : Freezable {
            obj.Freeze();
            return obj;
        }
    }
}