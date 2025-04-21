using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.Services;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace AssistenciaTecnicaApp.ViewModels
{
    public class CustomersViewModel : ViewModelBase
    {
        private enum CustomerViewMode
        {
            List,
            Add,
            Search
        }
        
        private readonly CustomerService? _customerService;
        
        // Semáforo para evitar múltiplas operações de carregamento simultâneas
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);
        
        [Reactive] public string Title { get; private set; } = "Clientes";
        [Reactive] public bool IsListMode { get; private set; } = true;
        [Reactive] public bool IsAddMode { get; private set; } = false;
        [Reactive] public bool IsSearchMode { get; private set; } = false;
        
        [Reactive] public CustomerViewModel? NewCustomer { get; set; }
        [Reactive] public ObservableCollection<Customer> Customers { get; set; } = new();
        
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; } = false;
        [Reactive] public string ErrorMessage { get; set; } = string.Empty;
        [Reactive] public bool HasError { get; set; } = false;
        
        public ICommand? AddCustomerCommand { get; private set; }
        public ICommand? ListCustomersCommand { get; private set; }
        public ICommand? SearchCustomersCommand { get; private set; }
        public ICommand? PerformSearchCommand { get; private set; }
        public ICommand? EditCustomerCommand { get; private set; }
        public ICommand? DeleteCustomerCommand { get; private set; }
        public ICommand? RefreshCommand { get; private set; }
        
        // Definição de comandos thread-safe
        private class ThreadSafeCommand : ICommand
        {
            private readonly Action _action;
            private readonly Func<bool>? _canExecute;
            private bool _isExecuting = false;
            
            public ThreadSafeCommand(Action action, Func<bool>? canExecute = null)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
                _canExecute = canExecute;
            }
            
            public event EventHandler? CanExecuteChanged;
            
            public bool CanExecute(object? parameter) 
            {
                return !_isExecuting && (_canExecute?.Invoke() ?? true);
            }
            
            public void Execute(object? parameter)
            {
                if (_isExecuting || !CanExecute(parameter))
                {
                    return;
                }
                
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    
                    // Garantir execução na thread UI
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        ExecuteCore();
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(ExecuteCore);
                    }
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
            
            private void ExecuteCore()
            {
                try
                {
                    _action();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[ThreadSafeCommand] Erro durante execução de comando");
                }
            }
            
            public void RaiseCanExecuteChanged()
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
                }
            }
        }
        
        // Implementação para comandos com parâmetros
        private class ThreadSafeCommand<T> : ICommand
        {
            private readonly Action<T?> _action;
            private readonly Func<T?, bool>? _canExecute;
            private bool _isExecuting = false;
            
            public ThreadSafeCommand(Action<T?> action, Func<T?, bool>? canExecute = null)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
                _canExecute = canExecute;
            }
            
            public event EventHandler? CanExecuteChanged;
            
            public bool CanExecute(object? parameter)
            {
                return !_isExecuting && (_canExecute?.Invoke(parameter is T t ? t : default) ?? true);
            }
            
            public void Execute(object? parameter)
            {
                if (_isExecuting || !CanExecute(parameter))
                {
                    return;
                }
                
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    
                    // Garantir execução na thread UI
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        ExecuteCore(parameter);
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() => ExecuteCore(parameter));
                    }
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
            
            private void ExecuteCore(object? parameter)
            {
                try
                {
                    _action(parameter is T t ? t : default);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[ThreadSafeCommand<T>] Erro durante execução de comando com parâmetro");
                }
            }
            
            public void RaiseCanExecuteChanged()
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Dispatcher.UIThread.Post(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
                }
            }
        }
        
        public CustomersViewModel()
        {
            Log.Debug("[CustomersViewModel] Initializing customer view model");
            
            try
            {
                // Get service from dependency injection
                _customerService = App.ServiceProvider?.GetService<CustomerService>();
                
                // Inicializar NewCustomer - garantir que nunca seja nulo
                NewCustomer = new CustomerViewModel();
                Log.Debug("[CustomersViewModel] NewCustomer initialized: {IsNull}", NewCustomer == null);
                
                // Criar comandos usando nossa implementação thread-safe
                try {
                    AddCustomerCommand = new ThreadSafeCommand(
                        () => SetMode(CustomerViewMode.Add));
                    Log.Debug("[CustomersViewModel] AddCustomerCommand created: {IsNull}", AddCustomerCommand == null);
                    
                    ListCustomersCommand = new ThreadSafeCommand(
                        () => SetMode(CustomerViewMode.List));
                    Log.Debug("[CustomersViewModel] ListCustomersCommand created: {IsNull}", ListCustomersCommand == null);
                    
                    SearchCustomersCommand = new ThreadSafeCommand(
                        () => SetMode(CustomerViewMode.Search));
                    Log.Debug("[CustomersViewModel] SearchCustomersCommand created: {IsNull}", SearchCustomersCommand == null);
                    
                    PerformSearchCommand = new ThreadSafeCommand(
                        async () => await SearchCustomersAsync());
                    
                    EditCustomerCommand = new ThreadSafeCommand<Customer>(
                        customer => EditCustomer(customer));
                    
                    DeleteCustomerCommand = new ThreadSafeCommand<Customer>(
                        async customer => await DeleteCustomerAsync(customer));
                    
                    RefreshCommand = new ThreadSafeCommand(
                        async () => await LoadCustomersAsync());
                }
                catch (Exception cmdEx) {
                    Log.Error(cmdEx, "[CustomersViewModel] Error creating commands");
                }
                
                // Start with the list view
                try {
                    SetMode(CustomerViewMode.List);
                    Log.Debug("[CustomersViewModel] Initial mode set to List");
                }
                catch (Exception modeEx) {
                    Log.Error(modeEx, "[CustomersViewModel] Error setting initial mode");
                }
                
                // Load customers or sample data - usando await Task.Run para evitar bloqueio da UI
                try {
                    _ = Task.Run(async () => {
                        await LoadCustomersAsync();
                    });
                    Log.Debug("[CustomersViewModel] Started background loading of customers");
                }
                catch (Exception loadEx) {
                    Log.Error(loadEx, "[CustomersViewModel] Error starting customer loading");
                }
                
                Log.Debug("[CustomersViewModel] Customer view model initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error initializing customer view model");
                HasError = true;
                ErrorMessage = "Erro ao inicializar. Verifique os logs.";
            }
        }
        
        private void SetMode(CustomerViewMode mode)
        {
            try
            {
                Log.Debug("[CustomersViewModel] Setting view mode to {Mode}", mode);
                
                // Sempre usar o Dispatcher.UIThread.Post para garantir que as mudanças de UI ocorram na thread correta
                Dispatcher.UIThread.Post(() => {
                    try {
                        UpdateModeInternal(mode);
                    }
                    catch (Exception ex) {
                        Log.Error(ex, "[CustomersViewModel] Error in UpdateModeInternal for mode {Mode}", mode);
                        SafeUpdateErrorState(true, "Erro ao mudar de visualização.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error setting view mode to {Mode}", mode);
                SafeUpdateErrorState(true, "Erro ao mudar de visualização.");
            }
        }
        
        private void UpdateModeInternal(CustomerViewMode mode)
        {
            IsListMode = mode == CustomerViewMode.List;
            IsAddMode = mode == CustomerViewMode.Add;
            IsSearchMode = mode == CustomerViewMode.Search;
            
            if (mode == CustomerViewMode.List)
            {
                // Carregar clientes sem bloquear a UI
                _ = Task.Run(async () => await LoadCustomersAsync());
            }
            
            Title = mode switch
            {
                CustomerViewMode.List => "Lista de Clientes",
                CustomerViewMode.Add => "Cadastro de Cliente",
                CustomerViewMode.Search => "Busca de Cliente",
                _ => "Clientes"
            };
            
            // Clear any previous errors
            HasError = false;
            ErrorMessage = string.Empty;
        }
        
        // Método auxiliar para atualizar estado de erro com segurança
        private void SafeUpdateErrorState(bool hasError, string message)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                HasError = hasError;
                ErrorMessage = message;
            }
            else
            {
                Dispatcher.UIThread.Post(() => 
                {
                    HasError = hasError;
                    ErrorMessage = message;
                });
            }
        }
        
        private async Task LoadCustomersAsync()
        {
            // Verificar se já há uma operação de carregamento em andamento
            if (!await _loadingSemaphore.WaitAsync(0))
            {
                Log.Debug("[CustomersViewModel] Customer loading operation already in progress, skipping");
                return;
            }
            
            try
            {
                Log.Debug("[CustomersViewModel] Loading customers");
                
                // Atualizar estado de carregamento na thread da UI
                SafeUpdateLoadingState(true);
                SafeUpdateErrorState(false, string.Empty);
                
                if (_customerService != null)
                {
                    Log.Debug("[CustomersViewModel] Using customer service to load data");
                    var customers = await _customerService.GetAllCustomersAsync();
                    
                    // Atualizar coleção na thread da UI
                    UpdateCustomersCollection(customers);
                    
                    Log.Debug("[CustomersViewModel] Loaded {Count} customers from service", customers.Count());
                }
                else
                {
                    Log.Debug("[CustomersViewModel] Customer service not available, loading sample data");
                    // LoadSampleData já deve chamar SafeUpdateCollection internamente
                    LoadSampleData();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error loading customers");
                SafeUpdateErrorState(true, "Erro ao carregar clientes.");
            }
            finally
            {
                SafeUpdateLoadingState(false);
                _loadingSemaphore.Release();
            }
        }
        
        // Método auxiliar para atualizar estado de carregamento com segurança
        private void SafeUpdateLoadingState(bool isLoading)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                IsLoading = isLoading;
            }
            else
            {
                Dispatcher.UIThread.Post(() => IsLoading = isLoading);
            }
        }
        
        // Método auxiliar para atualizar a coleção de clientes com segurança
        private void UpdateCustomersCollection(IEnumerable<Customer> customers)
        {
            Dispatcher.UIThread.Post(() => {
                try {
                    Customers.Clear();
                    foreach (var customer in customers)
                    {
                        Customers.Add(customer);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex, "[CustomersViewModel] Error updating customers collection in UI thread");
                }
            });
        }
        
        private async Task SearchCustomersAsync()
        {
            // Verificar se já há uma operação de carregamento em andamento
            if (!await _loadingSemaphore.WaitAsync(0))
            {
                Log.Debug("[CustomersViewModel] Search operation already in progress, skipping");
                return;
            }
            
            try
            {
                Log.Debug("[CustomersViewModel] Searching customers with term: {SearchTerm}", SearchTerm);
                
                // Atualizar estado de carregamento na thread da UI
                SafeUpdateLoadingState(true);
                SafeUpdateErrorState(false, string.Empty);
                
                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    // Se o termo de busca estiver vazio, carregue todos os clientes
                    await LoadCustomersAsync();
                    return;
                }
                
                if (_customerService != null)
                {
                    var customers = await _customerService.SearchCustomersAsync(SearchTerm);
                    
                    // Usar o dispatcher para atualizar a UI
                    Dispatcher.UIThread.Post(() => {
                        try {
                            Customers.Clear();
                            foreach (var customer in customers)
                            {
                                Customers.Add(customer);
                            }
                            
                            if (customers.Count() == 0)
                            {
                                SafeUpdateErrorState(true, "Nenhum cliente encontrado com os critérios de busca.");
                            }
                            
                            Log.Debug("[CustomersViewModel] Search found {Count} customers", customers.Count());
                        }
                        catch (Exception ex) {
                            Log.Error(ex, "[CustomersViewModel] Error updating UI with search results");
                        }
                    });
                }
                else
                {
                    Log.Debug("[CustomersViewModel] Customer service not available for search");
                    SafeUpdateErrorState(true, "Serviço de clientes não disponível para busca.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error searching customers with term: {SearchTerm}", SearchTerm);
                SafeUpdateErrorState(true, "Erro ao buscar clientes.");
            }
            finally
            {
                SafeUpdateLoadingState(false);
                _loadingSemaphore.Release();
            }
        }
        
        private void EditCustomer(Customer? customer)
        {
            if (customer == null)
            {
                Log.Warning("[CustomersViewModel] Attempted to edit null customer");
                SafeUpdateErrorState(true, "Cliente não selecionado para edição.");
                return;
            }
            
            try
            {
                Log.Debug("[CustomersViewModel] Editing customer: {CustomerName}", customer.Name);
                
                // Garantir que operamos na UI thread
                Dispatcher.UIThread.Post(() => {
                    try {
                        if (NewCustomer != null)
                        {
                            NewCustomer.LoadFromCustomer(customer);
                            SetMode(CustomerViewMode.Add);
                        }
                        else
                        {
                            Log.Warning("[CustomersViewModel] NewCustomer is null, cannot edit customer");
                            SafeUpdateErrorState(true, "Erro ao carregar cliente para edição.");
                        }
                    }
                    catch (Exception ex) {
                        Log.Error(ex, "[CustomersViewModel] Error in edit customer UI thread operation");
                        SafeUpdateErrorState(true, "Erro ao processar edição do cliente.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error editing customer: {CustomerName}", customer.Name);
                SafeUpdateErrorState(true, "Erro ao editar cliente.");
            }
        }
        
        private async Task DeleteCustomerAsync(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    Log.Warning("[CustomersViewModel] Attempted to delete null customer");
                    SafeUpdateErrorState(true, "Cliente inválido para exclusão.");
                    return;
                }
                
                Log.Debug("[CustomersViewModel] Deleting customer ID: {Id}", customer.Id);
                
                if (_customerService != null)
                {
                    var result = await _customerService.DeleteCustomerAsync(customer.Id);
                    if (result)
                    {
                        Log.Information("[CustomersViewModel] Successfully deleted customer ID: {Id}", customer.Id);
                        
                        // Remover cliente da coleção de forma segura
                        Dispatcher.UIThread.Post(() => 
                        {
                            Customers.Remove(customer);
                        });
                    }
                    else
                    {
                        Log.Warning("[CustomersViewModel] Failed to delete customer ID: {Id}", customer.Id);
                        SafeUpdateErrorState(true, "Falha ao excluir o cliente.");
                    }
                }
                else
                {
                    // Simple client-side removal if service is not available
                    Log.Debug("[CustomersViewModel] Customer service not available, removing customer from local collection");
                    
                    // Remover cliente da coleção de forma segura
                    Dispatcher.UIThread.Post(() => 
                    {
                        Customers.Remove(customer);
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error deleting customer ID: {Id}", customer?.Id);
                SafeUpdateErrorState(true, "Erro ao excluir cliente.");
            }
        }
        
        private void LoadSampleData()
        {
            try
            {
                Log.Debug("[CustomersViewModel] Loading sample customer data");
                
                var sampleCustomers = new List<Customer>
                {
                    new Customer
                    {
                        Id = 1,
                        Name = "João Silva",
                        Document = "123.456.789-00",
                        Type = CustomerType.Individual,
                        Phone = "(11) 98765-4321",
                        Email = "joao@example.com",
                        Address = "Rua Principal, 123",
                        City = "São Paulo",
                        State = "SP",
                        ZipCode = "01234-567",
                        CreatedAt = DateTime.Now.AddDays(-30)
                    },
                    new Customer
                    {
                        Id = 2,
                        Name = "ABC Comércio Ltda",
                        Document = "12.345.678/0001-90",
                        Type = CustomerType.Company,
                        Phone = "(11) 3456-7890",
                        Email = "contato@abccomercio.com",
                        Address = "Av. Comercial, 456",
                        City = "São Paulo",
                        State = "SP",
                        ZipCode = "04567-890",
                        CreatedAt = DateTime.Now.AddDays(-15)
                    }
                };
                
                // Atualizar a coleção com os dados de exemplo
                UpdateCustomersCollection(sampleCustomers);
                
                Log.Debug("[CustomersViewModel] Loaded {Count} sample customers", sampleCustomers.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersViewModel] Error loading sample data");
                SafeUpdateErrorState(true, "Erro ao carregar dados de exemplo.");
            }
        }
    }
} 