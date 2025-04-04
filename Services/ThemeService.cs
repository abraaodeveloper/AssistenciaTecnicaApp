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
            
            // Aplicar o tema quando o serviço é instanciado
            ApplyTheme(_currentTheme);
        }
        
        public void ApplyTheme(string themeKey)
        {
            try
            {
                // Simplificando para usar apenas referências diretas
                var theme = _app.Resources[themeKey];
                if (theme is ResourceDictionary themeDict)
                {
                    // Obtém todas as chaves de cores do tema
                    foreach (var key in themeDict.Keys)
                    {
                        if (themeDict[key] is Color colorValue)
                        {
                            // Atualiza a cor dinamicamente nos recursos da aplicação
                            _app.Resources[key] = colorValue;
                        }
                    }
                    
                    Log.Information("Tema aplicado: {Theme}", themeKey);
                }
                else
                {
                    Log.Warning("Tema não encontrado: {Theme}", themeKey);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao aplicar o tema: {Theme}", themeKey);
            }
        }
        
        public bool SaveThemeConfig()
        {
            bool result = _themeConfig.SaveConfig();
            if (result)
            {
                Log.Information("Configuração de tema salva com sucesso");
            }
            else
            {
                Log.Warning("Erro ao salvar configuração de tema");
            }
            return result;
        }
    }
} 