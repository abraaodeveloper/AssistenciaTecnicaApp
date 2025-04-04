using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using AssistenciaTecnicaApp.Models;
using Serilog;
using ReactiveUI;

namespace AssistenciaTecnicaApp.Services
{
    /// <summary>
    /// Service responsible for managing and applying application themes.
    /// </summary>
    public class ThemeService : ReactiveObject
    {
        private ThemeConfig _themeConfig;
        private string _currentTheme;
        private Avalonia.Application _app;
        
        public ThemeConfig ThemeConfig => _themeConfig;
        
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentTheme, value);
                _themeConfig.CurrentTheme = value;
                ApplyTheme(value);
            }
        }
        
        public ThemeService(Avalonia.Application app)
        {
            _app = app;
            _themeConfig = ThemeConfig.LoadConfig();
            _currentTheme = _themeConfig.CurrentTheme;
            
            // Apply theme when service is instantiated
            ApplyTheme(_currentTheme);
        }
        
        /// <summary>
        /// Applies the specified theme by updating application resources.
        /// </summary>
        /// <param name="themeKey">The key of the theme to apply</param>
        public void ApplyTheme(string themeKey)
        {
            try
            {
                // Use direct references to simplify
                var theme = _app.Resources[themeKey];
                if (theme is ResourceDictionary themeDict)
                {
                    // Get all theme color keys
                    foreach (var key in themeDict.Keys)
                    {
                        if (themeDict[key] is Color colorValue)
                        {
                            // Update color dynamically in application resources
                            _app.Resources[key] = colorValue;
                        }
                    }
                    
                    Log.Information("Theme applied: {Theme}", themeKey);
                }
                else
                {
                    Log.Warning("Theme not found: {Theme}", themeKey);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying theme: {Theme}", themeKey);
            }
        }
        
        /// <summary>
        /// Saves the current theme configuration.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool SaveThemeConfig()
        {
            bool result = _themeConfig.SaveConfig();
            if (result)
            {
                Log.Information("Theme configuration saved successfully");
            }
            else
            {
                Log.Warning("Error saving theme configuration");
            }
            return result;
        }
    }
} 