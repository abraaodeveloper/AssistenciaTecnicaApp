using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AssistenciaTecnicaApp.ViewModels;
using System;
using Avalonia.Platform;

namespace AssistenciaTecnicaApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();

            try
            {
                var uri = new Uri("avares://AssistenciaTecnicaApp/Assets/logo.png");
                var assets = AssetLoader.Open(uri);
                Icon = new WindowIcon(assets);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar ícone: {ex.Message}");
            }

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 