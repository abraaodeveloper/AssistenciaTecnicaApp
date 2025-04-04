using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using ReactiveUI;

namespace AssistenciaTecnicaApp.Models
{
    public class ThemeConfig : ReactiveObject
    {
        private string _currentTheme = "MatPhoneTheme";
        
        public string CurrentTheme 
        { 
            get => _currentTheme;
            set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
        }
        
        public List<ThemeInfo> AvailableThemes { get; } = new List<ThemeInfo>
        {
            new ThemeInfo("MatPhoneTheme", "MatPhone (Padrão)", "#F07F2E"),
            new ThemeInfo("BlueTheme", "Azul Corporativo", "#2B579A"),
            new ThemeInfo("DarkTheme", "Modo Escuro", "#BB86FC")
        };
        
        public bool SaveConfig()
        {
            try
            {
                var configDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AssistenciaTecnicaApp"
                );
                
                Directory.CreateDirectory(configDir);
                
                var filePath = Path.Combine(configDir, "theme.json");
                var json = JsonSerializer.Serialize(new { Theme = CurrentTheme });
                
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static ThemeConfig LoadConfig()
        {
            try
            {
                var filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AssistenciaTecnicaApp", 
                    "theme.json"
                );
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (data.TryGetProperty("Theme", out var themeValue))
                    {
                        return new ThemeConfig { CurrentTheme = themeValue.GetString() ?? "MatPhoneTheme" };
                    }
                }
                
                return new ThemeConfig();
            }
            catch (Exception)
            {
                return new ThemeConfig();
            }
        }
    }
    
    public class ThemeInfo
    {
        public string Id { get; }
        public string Name { get; }
        public string ColorHex { get; }
        
        public ThemeInfo(string id, string name, string colorHex)
        {
            Id = id;
            Name = name;
            ColorHex = colorHex;
        }
    }
} 