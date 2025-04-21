using AssistenciaTecnicaApp.Models;
using Avalonia.Data.Converters;
using Serilog;
using System;
using System.Globalization;

namespace AssistenciaTecnicaApp.Converters
{
    /// <summary>
    /// Converts between enum CustomerType and ComboBox SelectedIndex
    /// </summary>
    public class EnumToIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                Log.Debug("[EnumToIntConverter] Converting from enum to int: {Value}", value);
                
                if (value is CustomerType customerType)
                {
                    int index = (int)customerType;
                    Log.Debug("[EnumToIntConverter] Converted enum {Value} to int {Index}", customerType, index);
                    return index;
                }
                
                // Em caso de falha, retorna Individual (0)
                Log.Warning("[EnumToIntConverter] Failed to convert value, returning default: 0");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[EnumToIntConverter] Error converting from enum to int: {Value}", value);
                return 0;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                Log.Debug("[EnumToIntConverter] Converting from int to enum: {Value}", value);
                
                if (value is int index)
                {
                    // Garantir que o índice está dentro dos limites do enum
                    int enumLength = Enum.GetNames(typeof(CustomerType)).Length;
                    if (index < 0 || index >= enumLength)
                    {
                        Log.Warning("[EnumToIntConverter] Index {Index} is out of bounds for CustomerType. Using default.", index);
                        return CustomerType.Individual;
                    }
                    
                    CustomerType customerType = (CustomerType)index;
                    Log.Debug("[EnumToIntConverter] Converted int {Index} to enum {Value}", index, customerType);
                    return customerType;
                }
                
                // Em caso de falha, retorna Individual
                Log.Warning("[EnumToIntConverter] Failed to convert back, returning default: CustomerType.Individual");
                return CustomerType.Individual;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[EnumToIntConverter] Error converting from int to enum: {Value}", value);
                return CustomerType.Individual;
            }
        }
    }
} 