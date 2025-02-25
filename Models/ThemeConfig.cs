using Avalonia.Media;
using System.Text.Json;
using System.IO;

namespace AssistenciaTecnicaApp.Models
{
    public class ThemeConfig
    {
        public Color PrimaryColor { get; set; } = Color.Parse("#F07F2E");
        public Color PrimaryDarkColor { get; set; } = Color.Parse("#D3661C");
        public Color SecondaryColor { get; set; } = Color.Parse("#F4F4F4");
        public Color TextPrimaryColor { get; set; } = Color.Parse("#1E1E1E");
        public Color TextSecondaryColor { get; set; } = Color.Parse("#666666");
        public Color SelectedColor { get; set; } = Color.Parse("#D9D9D9");
        public Color BorderColor { get; set; } = Color.Parse("#E0E0E0");
        public Color BackgroundColor { get; set; } = Color.Parse("#FFFFFF");
        // ... outras cores ...

        public static ThemeConfig LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ThemeConfig>(json) ?? new ThemeConfig();
        }

        public void SaveToFile(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(path, json);
        }
    }
} 