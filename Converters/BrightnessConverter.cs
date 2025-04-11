using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AfbeeldingenUitzoeken.Converters
{
    /// <summary>
    /// Converter that adjusts the brightness of a color.
    /// Used for consistent button hover effects throughout the application.
    /// </summary>
    public class BrightnessConverter : IValueConverter
    {
        /// <summary>
        /// Factor by which to increase brightness (values > 1 brighten, values < 1 darken)
        /// </summary>
        public double BrightnessFactor { get; set; } = 1.2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                System.Windows.Media.Color originalColor = brush.Color;
                
                // Convert to HSL to adjust brightness while maintaining hue
                double r = originalColor.R / 255.0;
                double g = originalColor.G / 255.0;
                double b = originalColor.B / 255.0;
                
                double max = Math.Max(r, Math.Max(g, b));
                double min = Math.Min(r, Math.Min(g, b));
                
                // Adjust brightness while maintaining hue and saturation
                double adjustedR = Math.Min(255, originalColor.R * BrightnessFactor);
                double adjustedG = Math.Min(255, originalColor.G * BrightnessFactor);
                double adjustedB = Math.Min(255, originalColor.B * BrightnessFactor);
                
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)adjustedR,
                    (byte)adjustedG,
                    (byte)adjustedB));
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
