using System;
using System.Windows.Input;
using System.Linq;
using ReactiveUI;
using Avalonia.Media;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.Services;

namespace AssistenciaTecnicaApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeService? _themeService;
        private ThemeInfo? _selectedTheme;
        private string _statusMessage = string.Empty;
        private IBrush _statusMessageColor = new SolidColorBrush(Colors.Transparent);
        
        public string Title => "Configurações";
        
        public ThemeConfig? ThemeConfig => _themeService?.ThemeConfig;
        
        public ThemeInfo? SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }
        
        public IBrush StatusMessageColor
        {
            get => _statusMessageColor;
            set => this.RaiseAndSetIfChanged(ref _statusMessageColor, value);
        }
        
        public ICommand SaveThemeCommand { get; }
        
        public SettingsViewModel()
        {
            // Use o App.ServiceProvider para obter o serviço
            if (App.ServiceProvider != null)
            {
                _themeService = App.ServiceProvider.GetService(typeof(ThemeService)) as ThemeService;
                
                // Inicializar o tema selecionado com base no tema atual
                if (_themeService?.ThemeConfig != null)
                {
                    var currentThemeId = _themeService.CurrentTheme;
                    _selectedTheme = _themeService.ThemeConfig.AvailableThemes
                        .FirstOrDefault(t => t.Id == currentThemeId) ?? 
                        _themeService.ThemeConfig.AvailableThemes.FirstOrDefault();
                }
            }
            
            // Comandos
            SaveThemeCommand = ReactiveCommand.Create(SaveTheme);
        }
        
        private void SaveTheme()
        {
            if (_selectedTheme != null && _themeService != null)
            {
                _themeService.CurrentTheme = _selectedTheme.Id;
                bool saved = _themeService.SaveThemeConfig();
                
                if (saved)
                {
                    StatusMessage = "Tema aplicado com sucesso!";
                    StatusMessageColor = new SolidColorBrush(Color.Parse("#4CAF50")); // Verde
                }
                else
                {
                    StatusMessage = "Erro ao salvar o tema. Tente novamente.";
                    StatusMessageColor = new SolidColorBrush(Color.Parse("#F44336")); // Vermelho
                }
            }
        }
    }
} 