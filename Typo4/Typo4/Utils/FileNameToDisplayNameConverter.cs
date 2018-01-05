using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Typo4.Utils {
    public class FileNameToDisplayNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Path.GetFileNameWithoutExtension(value?.ToString() ?? "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}