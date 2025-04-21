using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;

namespace AssistenciaTecnicaApp.Converters
{
    public class IconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string iconName)
            {
                try
                {
                    // Primeiro, verificar no dicionário de recursos para compatibilidade com ícones antigos
                    if (Application.Current?.Resources.ContainsKey($"{iconName}_regular") == true)
                    {
                        return Application.Current?.Resources[$"{iconName}_regular"] as StreamGeometry;
                    }
                    
                    // Tentar carregar do diretório de menu
                    string menuPath = $"avares://AssistenciaTecnicaApp/Assets/Icons/Menu/{iconName}.svg";
                    if (AssetLoader.Exists(new Uri(menuPath)))
                    {
                        return LoadSvgPath(menuPath);
                    }
                    
                    // Tentar carregar do diretório de submenu
                    string submenuPath = $"avares://AssistenciaTecnicaApp/Assets/Icons/SubMenu/{iconName}.svg";
                    if (AssetLoader.Exists(new Uri(submenuPath)))
                    {
                        return LoadSvgPath(submenuPath);
                    }
                }
                catch (Exception ex)
                {
                    // Falha ao carregar o ícone - usar fallback
                    System.Diagnostics.Debug.WriteLine($"Erro ao carregar ícone: {ex.Message}");
                }
            }
            
            // Fallback para um ícone padrão se necessário
            return Application.Current?.Resources["logo"] as StreamGeometry;
        }

        private StreamGeometry? LoadSvgPath(string assetPath)
        {
            try
            {
                using var stream = AssetLoader.Open(new Uri(assetPath));
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();
                
                // Extrair o path do SVG
                int pathStart = content.IndexOf("<path d=\"");
                if (pathStart >= 0)
                {
                    pathStart += 9; // mover para após "<path d=\""
                    int pathEnd = content.IndexOf("\"", pathStart);
                    if (pathEnd > pathStart)
                    {
                        string pathData = content.Substring(pathStart, pathEnd - pathStart);
                        var geometry = StreamGeometry.Parse(pathData);
                        return geometry;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar SVG: {ex.Message}");
            }
            
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 