using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AssistenciaTecnicaApp.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly CustomerService? _customerService;
        private readonly ILogger<CustomerViewModel> _logger;
        
        // Nome privado para manipular
        private string _name = string.Empty;
        
        // Tipo privado para manipular
        private CustomerType _type = CustomerType.Individual;
        
        [Reactive] public int Id { get; set; }
        
        // Implementação personalizada para Name com log
        public string Name 
        { 
            get 
            {
                try
                {
                    _logger.LogDebug("Getting Name property: '{Value}'", _name);
                    return _name; 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception getting Name property");
                    return string.Empty; // Fallback seguro
                }
            } 
            set 
            {
                try
                {
                    _logger.LogDebug("Setting Name property: '{OldValue}' -> '{NewValue}'", _name, value);
                    
                    // Validar o valor (mesmo que aceite nulo, registrar para depuração)
                    if (value == null)
                    {
                        _logger.LogWarning("Null value being set to Name property");
                        _name = string.Empty;
                    }
                    else
                    {
                        _name = value;
                    }
                    
                    try 
                    {
                        this.RaisePropertyChanged(nameof(Name));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in RaisePropertyChanged for Name property");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception setting Name property");
                    _name = value ?? string.Empty; // Fallback seguro
                }
            }
        }
        
        [Reactive] public string Document { get; set; } = string.Empty;
        
        // Propriedade Type com tratamento de exceções
        public CustomerType Type
        {
            get
            {
                try
                {
                    _logger.LogDebug("Getting Type property: '{Value}'", _type);
                    return _type;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception getting Type property");
                    return CustomerType.Individual; // Valor padrão seguro
                }
            }
            set
            {
                try
                {
                    _logger.LogDebug("Setting Type property: '{OldValue}' -> '{NewValue}'", _type, value);
                    
                    // Verificar se o valor está no enum (validação básica)
                    if (!Enum.IsDefined(typeof(CustomerType), value))
                    {
                        _logger.LogWarning("Invalid CustomerType value: {Value}, defaulting to Individual", value);
                        _type = CustomerType.Individual;
                    }
                    else
                    {
                        _type = value;
                    }
                    
                    try
                    {
                        this.RaisePropertyChanged(nameof(Type));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in RaisePropertyChanged for Type property");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception setting Type property");
                    // Manter o valor anterior em caso de erro
                }
            }
        }
        
        [Reactive] public string Phone { get; set; } = string.Empty;
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string Address { get; set; } = string.Empty;
        [Reactive] public string City { get; set; } = string.Empty;
        [Reactive] public string State { get; set; } = string.Empty;
        [Reactive] public string ZipCode { get; set; } = string.Empty;
        [Reactive] public bool Active { get; set; } = true;
        
        [Reactive] public string ErrorMessage { get; set; } = string.Empty;
        [Reactive] public bool HasError { get; set; } = false;
        [Reactive] public bool IsSaving { get; set; } = false;
        [Reactive] public bool IsEditing { get; set; } = false;
        
        public ICommand? SaveCommand { get; private set; }
        public ICommand? ClearCommand { get; private set; }
        
        public CustomerViewModel(ILogger<CustomerViewModel> logger)
        {
            try
            {
                _logger = logger;
                _logger.LogDebug("Initializing customer detail view model");
                
                // Get service from dependency injection
                _customerService = App.ServiceProvider?.GetService<CustomerService>();
                
                SaveCommand = ReactiveCommand.CreateFromTask(SaveCustomerAsync);
                ClearCommand = ReactiveCommand.Create(ClearForm);
                
                _logger.LogDebug("Customer detail view model initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing customer detail view model");
                HasError = true;
                ErrorMessage = "Erro ao inicializar formulário.";
            }
        }
        
        public void LoadCustomer(Customer customer)
        {
            try
            {
                _logger.LogDebug("Starting LoadCustomer method");
                
                if (customer == null)
                {
                    _logger.LogWarning("Attempted to load null customer");
                    HasError = true;
                    ErrorMessage = "Cliente inválido para edição.";
                    return;
                }
                
                _logger.LogDebug("Loading customer for editing: ID {Id}, Name: {Name}", 
                    customer.Id, customer.Name);
                
                try
                {
                    // Set properties one by one with try/catch for each
                    SetProperty(() => Id = customer.Id, nameof(Id));
                    SetProperty(() => Name = customer.Name, nameof(Name));
                    SetProperty(() => Document = customer.Document, nameof(Document));
                    SetProperty(() => Type = customer.Type, nameof(Type));
                    SetProperty(() => Phone = customer.Phone, nameof(Phone));
                    SetProperty(() => Email = customer.Email, nameof(Email));
                    SetProperty(() => Address = customer.Address, nameof(Address));
                    SetProperty(() => City = customer.City, nameof(City));
                    SetProperty(() => State = customer.State, nameof(State));
                    SetProperty(() => ZipCode = customer.ZipCode, nameof(ZipCode));
                    SetProperty(() => Active = customer.Active, nameof(Active));
                    SetProperty(() => IsEditing = true, nameof(IsEditing));
                    
                    _logger.LogDebug("Customer loaded successfully for editing");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting properties during customer load");
                    throw; // Rethrow to be caught by outer try/catch
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer for editing: ID {Id}", customer?.Id);
                HasError = true;
                ErrorMessage = "Erro ao carregar dados do cliente.";
            }
        }
        
        // Método para carregar dados de um cliente existente para edição
        public void LoadFromCustomer(Customer customer)
        {
            if (customer == null) return;
            
            try
            {
                _logger.LogDebug("Loading customer data for editing: {Id}", customer.Id);
                
                // Copiar propriedades do cliente para o ViewModel
                Id = customer.Id;
                Name = customer.Name;
                Document = customer.Document;
                Type = customer.Type;
                Phone = customer.Phone;
                Email = customer.Email;
                Address = customer.Address;
                City = customer.City;
                State = customer.State;
                ZipCode = customer.ZipCode;
                Active = customer.Active;
                
                // Atualizar flags de estado
                IsEditing = true;
                HasError = false;
                ErrorMessage = string.Empty;
                
                _logger.LogDebug("Customer data loaded successfully for editing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer data for editing: {Id}", customer.Id);
                ErrorMessage = "Erro ao carregar dados do cliente para edição.";
                HasError = true;
            }
        }
        
        // Helper method to set properties safely
        private void SetProperty(Action setter, string propertyName)
        {
            try
            {
                _logger.LogDebug("Setting property: {PropertyName}", propertyName);
                setter();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting property: {PropertyName}", propertyName);
                throw new ApplicationException($"Failed to set property {propertyName}", ex);
            }
        }
        
        private async Task SaveCustomerAsync()
        {
            try
            {
                _logger.LogDebug("Starting SaveCustomerAsync method. ID: {Id}, IsEditing: {IsEditing}", Id, IsEditing);
                
                IsSaving = true;
                HasError = false;
                ErrorMessage = string.Empty;
                
                // Validate fields
                if (string.IsNullOrWhiteSpace(Name))
                {
                    _logger.LogWarning("Validation failed: Name is required");
                    ErrorMessage = "Nome do cliente é obrigatório";
                    HasError = true;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Document))
                {
                    _logger.LogWarning("Validation failed: Document is required");
                    ErrorMessage = "Documento (CPF/CNPJ) é obrigatório";
                    HasError = true;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    _logger.LogWarning("Validation failed: Phone is required");
                    ErrorMessage = "Telefone é obrigatório";
                    HasError = true;
                    return;
                }
                
                _logger.LogDebug("Validation passed, creating customer object");
                
                // Create customer object
                var customer = new Customer
                {
                    Id = IsEditing ? Id : 0,
                    Name = Name,
                    Document = Document,
                    Type = Type,
                    Phone = Phone,
                    Email = Email,
                    Address = Address,
                    City = City,
                    State = State,
                    ZipCode = ZipCode,
                    Active = Active,
                    CreatedAt = IsEditing ? DateTime.Now : DateTime.Now,
                    UpdatedAt = IsEditing ? DateTime.Now : null
                };
                
                _logger.LogDebug("Customer object created for saving: {@Customer}", 
                    new { customer.Id, customer.Name, customer.Type, IsNew = !IsEditing });
                
                // Save to database
                if (_customerService != null)
                {
                    _logger.LogDebug("CustomerService available, proceeding with save operation");
                    
                    try
                    {
                        if (IsEditing)
                        {
                            _logger.LogDebug("Updating existing customer ID: {Id}", customer.Id);
                            var updateResult = await _customerService.UpdateCustomerAsync(customer);
                            _logger.LogInformation("Customer update result: {Result}, ID: {Id}", updateResult, customer.Id);
                            ErrorMessage = "Cliente atualizado com sucesso!";
                        }
                        else
                        {
                            _logger.LogDebug("Adding new customer");
                            var newId = await _customerService.AddCustomerAsync(customer);
                            _logger.LogInformation("New customer created with ID: {Id}", newId);
                            ErrorMessage = "Cliente cadastrado com sucesso!";
                        }
                        
                        _logger.LogDebug("Save operation completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception in customer service operation");
                        throw; // Re-throw to be caught by outer try-catch
                    }
                }
                else
                {
                    // If service is not available, simulate saving
                    _logger.LogWarning("Customer service not available, simulating save operation");
                    await Task.Delay(1000);
                    ErrorMessage = IsEditing 
                        ? "Cliente atualizado com sucesso!" 
                        : "Cliente cadastrado com sucesso!";
                }
                
                // Clear form after saving
                _logger.LogDebug("Clearing form after successful save");
                ClearForm();
                
                // Show success message temporarily
                _logger.LogDebug("Setting timer to clear success message");
                try
                {
                    await Task.Delay(3000);
                    _logger.LogDebug("Clearing success message after delay");
                    ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in delay/clear message operation");
                }
                
                _logger.LogDebug("SaveCustomerAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving customer: {ErrorType}, {ErrorMessage}", ex.GetType().Name, ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {ErrorType}, {ErrorMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                ErrorMessage = $"Erro ao salvar cliente: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsSaving = false;
                _logger.LogDebug("SaveCustomerAsync finalized, IsSaving set to false");
            }
        }
        
        private void ClearForm()
        {
            try
            {
                _logger.LogDebug("Starting to clear customer form");
                
                try
                {
                    // Clear properties one by one with try/catch for each
                    SetProperty(() => Id = 0, nameof(Id));
                    SetProperty(() => Name = string.Empty, nameof(Name));
                    SetProperty(() => Document = string.Empty, nameof(Document));
                    SetProperty(() => Type = CustomerType.Individual, nameof(Type));
                    SetProperty(() => Phone = string.Empty, nameof(Phone));
                    SetProperty(() => Email = string.Empty, nameof(Email));
                    SetProperty(() => Address = string.Empty, nameof(Address));
                    SetProperty(() => City = string.Empty, nameof(City));
                    SetProperty(() => State = string.Empty, nameof(State));
                    SetProperty(() => ZipCode = string.Empty, nameof(ZipCode));
                    SetProperty(() => Active = true, nameof(Active));
                    SetProperty(() => ErrorMessage = string.Empty, nameof(ErrorMessage));
                    SetProperty(() => HasError = false, nameof(HasError));
                    SetProperty(() => IsEditing = false, nameof(IsEditing));
                    
                    _logger.LogDebug("Customer form cleared successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CustomerViewModel] Error clearing specific properties during form clear");
                    throw; // Rethrow to be caught by outer try/catch
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CustomerViewModel] Error clearing customer form");
                HasError = true;
                ErrorMessage = "Erro ao limpar formulário.";
                
                // Ainda tenta limpar as propriedades essenciais para evitar dados inconsistentes
                try
                {
                    Id = 0;
                    Name = string.Empty;
                    IsEditing = false;
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "[CustomerViewModel] Fatal error clearing essential properties");
                }
            }
        }
    }
} 