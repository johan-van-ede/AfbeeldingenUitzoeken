using System;
using System.Globalization;
using System.Windows.Data;

namespace AfbeeldingenUitzoeken.Converters
{
    /// <summary>
    /// Converter that adjusts opacity based on loading state
    /// </summary>
    public class LoadingOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If isLoading is true, return the reduced opacity value (parameter or default 0.5)
            if (value is bool isLoading && isLoading)
            {
                double opacity = 0.5; // Default reduced opacity
                
                // Try to parse parameter if provided
                if (parameter != null)
                {
                    double.TryParse(parameter.ToString(), out opacity);
                }
                
                return opacity;
            }
            
            // If not loading, return full opacity
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
