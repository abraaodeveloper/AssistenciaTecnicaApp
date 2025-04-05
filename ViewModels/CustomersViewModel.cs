using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AssistenciaTecnicaApp.Models;
using AssistenciaTecnicaApp.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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
        
        [Reactive] public string Title { get; private set; } = "Customers";
        [Reactive] public bool IsListMode { get; private set; } = true;
        [Reactive] public bool IsAddMode { get; private set; } = false;
        [Reactive] public bool IsSearchMode { get; private set; } = false;
        
        [Reactive] public CustomerViewModel NewCustomer { get; set; }
        [Reactive] public ObservableCollection<Customer> Customers { get; set; } = new();
        
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; } = false;
        
        public ICommand AddCustomerCommand { get; }
        public ICommand ListCustomersCommand { get; }
        public ICommand SearchCustomersCommand { get; }
        public ICommand PerformSearchCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand RefreshCommand { get; }
        
        public CustomersViewModel()
        {
            // Get service from dependency injection
            _customerService = App.ServiceProvider?.GetService<CustomerService>();
            
            NewCustomer = new CustomerViewModel();
            
            AddCustomerCommand = ReactiveCommand.Create(() => SetMode(CustomerViewMode.Add));
            ListCustomersCommand = ReactiveCommand.Create(() => SetMode(CustomerViewMode.List));
            SearchCustomersCommand = ReactiveCommand.Create(() => SetMode(CustomerViewMode.Search));
            
            PerformSearchCommand = ReactiveCommand.CreateFromTask(SearchCustomersAsync);
            EditCustomerCommand = ReactiveCommand.Create<Customer>(EditCustomer);
            DeleteCustomerCommand = ReactiveCommand.CreateFromTask<Customer>(DeleteCustomerAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadCustomersAsync);
            
            // Start with the list view
            SetMode(CustomerViewMode.List);
            
            // Load customers or sample data
            LoadCustomersAsync().ConfigureAwait(false);
        }
        
        private void SetMode(CustomerViewMode mode)
        {
            IsListMode = mode == CustomerViewMode.List;
            IsAddMode = mode == CustomerViewMode.Add;
            IsSearchMode = mode == CustomerViewMode.Search;
            
            if (mode == CustomerViewMode.List)
            {
                _ = LoadCustomersAsync();
            }
            
            Title = mode switch
            {
                CustomerViewMode.List => "Customer List",
                CustomerViewMode.Add => "Register Customer",
                CustomerViewMode.Search => "Search Customer",
                _ => "Customers"
            };
        }
        
        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                
                if (_customerService != null)
                {
                    var customers = await _customerService.GetAllCustomersAsync();
                    Customers.Clear();
                    foreach (var customer in customers)
                    {
                        Customers.Add(customer);
                    }
                }
                else
                {
                    LoadSampleData();
                }
            }
            catch (Exception ex)
            {
                // Handle error (would add logging here)
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task SearchCustomersAsync()
        {
            try
            {
                IsLoading = true;
                
                if (_customerService != null)
                {
                    var customers = await _customerService.SearchCustomersAsync(SearchTerm);
                    Customers.Clear();
                    foreach (var customer in customers)
                    {
                        Customers.Add(customer);
                    }
                }
                else
                {
                    // Simple client-side filtering if service is not available
                    var filteredCustomers = Customers.Where(c => 
                        c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        c.Document.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        c.Phone.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    Customers.Clear();
                    foreach (var customer in filteredCustomers)
                    {
                        Customers.Add(customer);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error (would add logging here)
                System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void EditCustomer(Customer customer)
        {
            NewCustomer.LoadCustomer(customer);
            SetMode(CustomerViewMode.Add);
        }
        
        private async Task DeleteCustomerAsync(Customer customer)
        {
            try
            {
                if (_customerService != null)
                {
                    var result = await _customerService.DeleteCustomerAsync(customer.Id);
                    if (result)
                    {
                        Customers.Remove(customer);
                    }
                }
                else
                {
                    // Simple client-side removal if service is not available
                    Customers.Remove(customer);
                }
            }
            catch (Exception ex)
            {
                // Handle error (would add logging here)
                System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
            }
        }
        
        private void LoadSampleData()
        {
            // Sample data for testing the interface
            Customers.Clear();
            
            Customers.Add(new Customer
            {
                Id = 1,
                Name = "John Smith",
                Document = "123-45-6789",
                Type = CustomerType.Individual,
                Phone = "(555) 123-4567",
                Email = "john@example.com",
                Address = "123 Main Street",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                CreatedAt = DateTime.Now.AddDays(-30)
            });
            
            Customers.Add(new Customer
            {
                Id = 2,
                Name = "ABC Corporation",
                Document = "12-3456789",
                Type = CustomerType.Company,
                Phone = "(555) 987-6543",
                Email = "contact@abccorp.com",
                Address = "456 Business Ave",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                CreatedAt = DateTime.Now.AddDays(-15)
            });
        }
    }
} 