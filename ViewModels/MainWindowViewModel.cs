using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.ViewModels;

namespace AssistenciaTecnicaApp.ViewModels;

public class NavMenuItem : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ViewModelBase? TargetPage { get; set; }
    public ObservableCollection<NavMenuItem> SubItems { get; set; } = new();
}

public class MainWindowViewModel : ViewModelBase
{
    private NavMenuItem? _selectedMenuItem;
    private User? _currentUser;
    private string _userName = "Abraão Martins";
    private string _userRole = "Administrador";

    public MainWindowViewModel()
    {
        SetupMenuItems();
    }

    public NavMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
    }

    public ObservableCollection<NavMenuItem> MenuItems { get; } = new();

    public User? CurrentUser
    {
        get => _currentUser;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _currentUser, value);
            if (value != null)
            {
                UserName = value.Nome;
                UserRole = value.Cargo.ToString();
            }
        }
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
        MenuItems.Add(new NavMenuItem
        {
            Name = "Início",
            Icon = "home_regular",
            TargetPage = new HomeViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Dashboard", Icon = "dashboard_regular", TargetPage = new HomeViewModel() },
                new NavMenuItem { Name = "Relatórios", Icon = "chart_regular", TargetPage = new HomeViewModel() }
            }
        });

        MenuItems.Add(new NavMenuItem
        {
            Name = "Ordens",
            Icon = "document_regular",
            TargetPage = new OrdersViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Nova Ordem", Icon = "add_regular", TargetPage = new OrdersViewModel() },
                new NavMenuItem { Name = "Listar Ordens", Icon = "list_regular", TargetPage = new OrdersViewModel() },
                new NavMenuItem { Name = "Buscar", Icon = "search_regular", TargetPage = new OrdersViewModel() }
            }
        });

        MenuItems.Add(new NavMenuItem
        {
            Name = "Clientes",
            Icon = "people_regular",
            TargetPage = new ClientsViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Novo Cliente", Icon = "add_regular", TargetPage = new ClientsViewModel() },
                new NavMenuItem { Name = "Listar Clientes", Icon = "list_regular", TargetPage = new ClientsViewModel() },
                new NavMenuItem { Name = "Buscar", Icon = "search_regular", TargetPage = new ClientsViewModel() }
            }
        });

        MenuItems.Add(new NavMenuItem
        {
            Name = "Configurações",
            Icon = "settings_regular",
            TargetPage = new SettingsViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Usuários", Icon = "people_regular", TargetPage = new SettingsViewModel() },
                new NavMenuItem { Name = "Lojas", Icon = "store_regular", TargetPage = new SettingsViewModel() },
                new NavMenuItem { Name = "Sistema", Icon = "settings_regular", TargetPage = new SettingsViewModel() }
            }
        });

        // Seleciona o primeiro item por padrão
        SelectedMenuItem = MenuItems[0];
    }
}
