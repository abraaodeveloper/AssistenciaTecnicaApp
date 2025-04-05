using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;

namespace AssistenciaTecnicaApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                string colors = parameter as string ?? "Red,Green";
                string[] colorParts = colors.Split(',');
                
                string colorName = boolValue ? colorParts[0] : colorParts.Length > 1 ? colorParts[1] : colorParts[0];
                
                if (targetType == typeof(IBrush) || targetType == typeof(Brush))
                {
                    // Verificar se existe um brush no dicionário de recursos
                    if (Application.Current?.Resources.TryGetResource(colorName + "Brush", null, out var brushResource) == true)
                    {
                        return brushResource;
                    }
                    
                    if (Application.Current?.Resources.TryGetResource(colorName, null, out var brush) == true)
                    {
                        return brush;
                    }
                    
                    // Tentar criar uma SolidColorBrush a partir do nome da cor
                    try
                    {
                        var color = Color.Parse(colorName);
                        return new SolidColorBrush(color);
                    }
                    catch
                    {
                        // Retornar uma cor padrão se não conseguir converter
                        return boolValue ? Brushes.Red : Brushes.Green;
                    }
                }
            }
            
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 