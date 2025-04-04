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
    private string _userName = "Abraão Martins";
    private string _userRole = "Administrador";

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

    public string UserName
    {
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    public string UserRole
    {
        get => _userRole;
        set => this.RaiseAndSetIfChanged(ref _userRole, value);
    }

    private void SetupMenuItems()
    {
        MenuItems.Add(new MenuItem
        {
            Name = "Início",
            Icon = "home_regular",
            TargetPage = new HomeViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Dashboard", Icon = "dashboard_regular" },
                new MenuItem { Name = "Relatórios", Icon = "chart_regular" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Ordens",
            Icon = "document_regular",
            TargetPage = new OrdersViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Nova Ordem", Icon = "add_regular" },
                new MenuItem { Name = "Listar Ordens", Icon = "list_regular" },
                new MenuItem { Name = "Buscar", Icon = "search_regular" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Clientes",
            Icon = "people_regular",
            TargetPage = new ClientsViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Novo Cliente", Icon = "add_regular" },
                new MenuItem { Name = "Listar Clientes", Icon = "list_regular" },
                new MenuItem { Name = "Buscar", Icon = "search_regular" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Configurações",
            Icon = "settings_regular",
            TargetPage = new SettingsViewModel(),
            SubItems =
            {
                new MenuItem { Name = "Usuários", Icon = "people_regular" },
                new MenuItem { Name = "Lojas", Icon = "store_regular" },
                new MenuItem { Name = "Sistema", Icon = "settings_regular" }
            }
        });

        // Seleciona o primeiro item por padrão
        SelectedMenuItem = MenuItems[0];
    }
}
