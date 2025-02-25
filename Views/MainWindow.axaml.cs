using Avalonia.Controls;
using System;
using Avalonia.Platform;

namespace AssistenciaTecnicaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

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
    }
}