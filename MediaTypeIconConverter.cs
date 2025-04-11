using System;
using System.Globalization;
using System.Windows.Data;
using AfbeeldingenUitzoeken.Models;

namespace AfbeeldingenUitzoeken
{
    public class MediaTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PictureModel media && media.IsVideo)
            {
                return "🎬"; // Film/video icon
            }
            return string.Empty; // Empty for images
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
