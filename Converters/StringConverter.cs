using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AssistenciaTecnicaApp.Converters
{
    public class StringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string options)
            {
                string[] parts = options.Split(',');
                
                if (value is bool boolValue)
                {
                    // For boolean values, return first option if true, second if false
                    return boolValue ? 
                        (parts.Length > 0 ? parts[0] : string.Empty) : 
                        (parts.Length > 1 ? parts[1] : string.Empty);
                }
                else if (value is int intValue && parts.Length > intValue && intValue >= 0)
                {
                    // For integer values, use as index into options
                    return parts[intValue];
                }
                else if (value is Enum enumValue)
                {
                    // For enum values, convert to int and use as index
                    int enumIndex = System.Convert.ToInt32(enumValue);
                    return enumIndex < parts.Length ? parts[enumIndex] : enumValue.ToString();
                }
            }
            
            return value?.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 