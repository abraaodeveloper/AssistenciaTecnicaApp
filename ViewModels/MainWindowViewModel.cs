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
    public object? Tag { get; set; }
}

public class MainWindowViewModel : ViewModelBase
{
    // Enum que representa os modos de visualização da tela de clientes
    private enum CustomerViewMode
    {
        List,
        Add,
        Search
    }
    
    private NavMenuItem? _selectedMenuItem;
    private NavMenuItem? _selectedSubItem;
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
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            if (value?.SubItems.Count > 0)
            {
                // Seleciona o primeiro sub-item por padrão
                SelectedSubItem = value.SubItems[0];
            }
        }
    }

    public NavMenuItem? SelectedSubItem
    {
        get => _selectedSubItem;
        set
        {
            var oldValue = _selectedSubItem;
            this.RaiseAndSetIfChanged(ref _selectedSubItem, value);
            
            if (value != null && oldValue != value)
            {
                HandleSubItemSelection(value);
            }
        }
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
    
    // Método para criar instâncias de CustomersViewModel com modos específicos
    private CustomersViewModel CreateCustomerViewWithMode(CustomerViewMode mode)
    {
        var viewModel = new CustomersViewModel();
        
        // Definir o modo da tela usando os comandos apropriados
        switch (mode)
        {
            case CustomerViewMode.Add:
                viewModel.AddCustomerCommand?.Execute(null);
                break;
            case CustomerViewMode.List:
                viewModel.ListCustomersCommand?.Execute(null);
                break;
            case CustomerViewMode.Search:
                viewModel.SearchCustomersCommand?.Execute(null);
                break;
        }
        
        return viewModel;
    }

    private void SetupMenuItems()
    {
        MenuItems.Add(new NavMenuItem
        {
            Name = "Início",
            Icon = "home",
            TargetPage = new HomeViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Dashboard", Icon = "dashboard", TargetPage = new HomeViewModel() },
                new NavMenuItem { Name = "Relatórios", Icon = "reports", TargetPage = new HomeViewModel() }
            }
        });

        MenuItems.Add(new NavMenuItem
        {
            Name = "Ordens",
            Icon = "orders",
            TargetPage = new OrdersViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Nova Ordem", Icon = "add", TargetPage = new OrdersViewModel() },
                new NavMenuItem { Name = "Listar Ordens", Icon = "list", TargetPage = new OrdersViewModel() },
                new NavMenuItem { Name = "Buscar", Icon = "search", TargetPage = new OrdersViewModel() }
            }
        });

        // Para Clientes, primeiro criar uma instância base da ViewModel que será compartilhada
        var customersViewModel = new CustomersViewModel();
        
        // Garantir que começa em modo de listagem
        customersViewModel.ListCustomersCommand?.Execute(null);
        
        MenuItems.Add(new NavMenuItem
        {
            Name = "Clientes",
            Icon = "clients",
            TargetPage = customersViewModel, // Usar a mesma instância como padrão
            SubItems =
            {
                new NavMenuItem { 
                    Name = "Lista de Clientes", 
                    Icon = "list", 
                    TargetPage = customersViewModel, // Mesma instância
                    Tag = "list"
                },
                new NavMenuItem { 
                    Name = "Novo Cliente", 
                    Icon = "add", 
                    TargetPage = customersViewModel, // Mesma instância
                    Tag = "add"
                },
                new NavMenuItem { 
                    Name = "Buscar Cliente", 
                    Icon = "search", 
                    TargetPage = customersViewModel, // Mesma instância
                    Tag = "search"
                }
            }
        });

        MenuItems.Add(new NavMenuItem
        {
            Name = "Configurações",
            Icon = "settings",
            TargetPage = new SettingsViewModel(),
            SubItems =
            {
                new NavMenuItem { Name = "Usuários", Icon = "users", TargetPage = new SettingsViewModel() },
                new NavMenuItem { Name = "Lojas", Icon = "store", TargetPage = new SettingsViewModel() },
                new NavMenuItem { Name = "Sistema", Icon = "system", TargetPage = new SettingsViewModel() }
            }
        });

        // Seleciona o primeiro item por padrão
        SelectedMenuItem = MenuItems[0];
    }

    private void HandleSubItemSelection(NavMenuItem subItem)
    {
        try
        {
            if (SelectedMenuItem?.Name == "Clientes" && subItem.TargetPage is CustomersViewModel viewModel)
            {
                // Usar a Tag para determinar o modo
                string tag = subItem.Tag as string ?? "";
                
                switch (tag)
                {
                    case "list":
                        viewModel.ListCustomersCommand?.Execute(null);
                        break;
                    case "add":
                        viewModel.AddCustomerCommand?.Execute(null);
                        break;
                    case "search":
                        viewModel.SearchCustomersCommand?.Execute(null);
                        break;
                }
            }
            
            // Sempre atualiza a página alvo para o do sub-item selecionado
            if (SelectedMenuItem != null)
            {
                SelectedMenuItem.TargetPage = subItem.TargetPage;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar seleção de sub-item: {ex.Message}");
        }
    }
}
