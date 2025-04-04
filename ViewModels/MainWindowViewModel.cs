using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.ViewModels;

namespace AssistenciaTecnicaApp.ViewModels;

public class MenuItem : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ViewModelBase? TargetPage { get; set; }
    public ObservableCollection<MenuItem> SubItems { get; set; } = new();
}

public class MainWindowViewModel : ViewModelBase
{
    private MenuItem? _selectedMenuItem;
    private User? _currentUser;

    public MainWindowViewModel()
    {
        SetupMenuItems();
    }

    public MenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    public User? CurrentUser
    {
        get => _currentUser;
        set => this.RaiseAndSetIfChanged(ref _currentUser, value);
    }

    private void SetupMenuItems()
    {
        var defaultIcon = "logo"; // Usando o mesmo ícone para todos

        MenuItems.Add(new MenuItem
        {
            Name = "Início",
            Icon = defaultIcon,
            TargetPage = new HomeViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Dashboard", Icon = defaultIcon },
                new MenuItem { Name = "Relatórios", Icon = defaultIcon }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Ordens",
            Icon = defaultIcon,
            TargetPage = new OrdersViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Nova Ordem", Icon = defaultIcon },
                new MenuItem { Name = "Listar Ordens", Icon = defaultIcon },
                new MenuItem { Name = "Buscar", Icon = defaultIcon }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Clientes",
            Icon = defaultIcon,
            TargetPage = new ClientsViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Novo Cliente", Icon = defaultIcon },
                new MenuItem { Name = "Listar Clientes", Icon = defaultIcon },
                new MenuItem { Name = "Buscar", Icon = defaultIcon }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Configurações",
            Icon = defaultIcon,
            TargetPage = new SettingsViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Usuários", Icon = defaultIcon },
                new MenuItem { Name = "Lojas", Icon = defaultIcon },
                new MenuItem { Name = "Sistema", Icon = defaultIcon }
            }
        });

        // Seleciona o primeiro item por padrão
        SelectedMenuItem = MenuItems[0];
    }
}
