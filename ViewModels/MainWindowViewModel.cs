using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.ViewModels;

namespace AssistenciaTecnicaApp.ViewModels;

public class MenuItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ViewModelBase? TargetPage { get; set; }
    public ObservableCollection<SubMenuItem> SubItems { get; set; } = new();
}

public class SubMenuItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ViewModelBase? TargetPage { get; set; }
}

public class MainWindowViewModel : ReactiveObject
{
    private ViewModelBase? _currentPage;
    private User? _currentUser;
    private ObservableCollection<SubMenuItem> _currentSubmenuItems;
    private MenuItem? _selectedMenuItem;

    public MainWindowViewModel()
    {
        _currentSubmenuItems = new ObservableCollection<SubMenuItem>();
        SetupMenuItems();
    }

    public MenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            if (value != null)
            {
                CurrentSubmenuItems.Clear();
                foreach (var item in value.SubItems)
                {
                    CurrentSubmenuItems.Add(item);
                }
                CurrentPage = value.TargetPage;
            }
        }
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public User? CurrentUser
    {
        get => _currentUser;
        set => this.RaiseAndSetIfChanged(ref _currentUser, value);
    }

    public ObservableCollection<SubMenuItem> CurrentSubmenuItems
    {
        get => _currentSubmenuItems;
        set => this.RaiseAndSetIfChanged(ref _currentSubmenuItems, value);
    }

    private void SetupMenuItems()
    {
        MenuItems.Add(new MenuItem
        {
            Name = "Início",
            Icon = "home",
            TargetPage = new HomeViewModel(),
            SubItems =
            {
                new SubMenuItem { Name = "Dashboard", Icon = "dashboard" },
                new SubMenuItem { Name = "Relatórios", Icon = "chart" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Ordens",
            Icon = "document",
            TargetPage = new OrdersViewModel(),
            SubItems =
            {
                new SubMenuItem { Name = "Nova Ordem", Icon = "add" },
                new SubMenuItem { Name = "Listar Ordens", Icon = "list" },
                new SubMenuItem { Name = "Buscar", Icon = "search" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Clientes",
            Icon = "people",
            TargetPage = new ClientsViewModel(),
            SubItems =
            {
                new SubMenuItem { Name = "Novo Cliente", Icon = "add" },
                new SubMenuItem { Name = "Listar Clientes", Icon = "list" },
                new SubMenuItem { Name = "Buscar", Icon = "search" }
            }
        });

        MenuItems.Add(new MenuItem
        {
            Name = "Configurações",
            Icon = "settings",
            TargetPage = new SettingsViewModel(),
            SubItems =
            {
                new SubMenuItem { Name = "Usuários", Icon = "people" },
                new SubMenuItem { Name = "Lojas", Icon = "store" },
                new SubMenuItem { Name = "Sistema", Icon = "settings" }
            }
        });

        // Seleciona o primeiro item por padrão
        SelectedMenuItem = MenuItems[0];
    }
}
