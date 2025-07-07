using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AfbeeldingenUitzoeken.Converters
{
    public class VideoThumbnailVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is bool isVideo &&
                values[1] is bool isPlaying)
            {
                return (isVideo && !isPlaying) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
