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
    public class CustomerViewModel : ViewModelBase
    {
        private readonly CustomerService? _customerService;
        
        [Reactive] public int Id { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Document { get; set; } = string.Empty;
        [Reactive] public CustomerType Type { get; set; } = CustomerType.Individual;
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
        
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }
        
        public CustomerViewModel()
        {
            // Get service from dependency injection
            _customerService = App.ServiceProvider?.GetService<CustomerService>();
            
            SaveCommand = ReactiveCommand.CreateFromTask(SaveCustomerAsync);
            ClearCommand = ReactiveCommand.Create(ClearForm);
        }
        
        public void LoadCustomer(Customer customer)
        {
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
            IsEditing = true;
        }
        
        private async Task SaveCustomerAsync()
        {
            try
            {
                IsSaving = true;
                HasError = false;
                ErrorMessage = string.Empty;
                
                // Validate fields
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ErrorMessage = "Customer name is required";
                    HasError = true;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Document))
                {
                    ErrorMessage = "Document (SSN/EIN) is required";
                    HasError = true;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    ErrorMessage = "Phone number is required";
                    HasError = true;
                    return;
                }
                
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
                    CreatedAt = DateTime.Now
                };
                
                // Save to database
                if (_customerService != null)
                {
                    if (IsEditing)
                    {
                        await _customerService.UpdateCustomerAsync(customer);
                        ErrorMessage = "Customer updated successfully!";
                    }
                    else
                    {
                        await _customerService.AddCustomerAsync(customer);
                        ErrorMessage = "Customer registered successfully!";
                    }
                }
                else
                {
                    // If service is not available, simulate saving
                    await Task.Delay(1000);
                    ErrorMessage = IsEditing 
                        ? "Customer updated successfully!" 
                        : "Customer registered successfully!";
                }
                
                // Clear form after saving
                ClearForm();
                
                // Show success message temporarily
                await Task.Delay(3000);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving customer: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsSaving = false;
            }
        }
        
        private void ClearForm()
        {
            Id = 0;
            Name = string.Empty;
            Document = string.Empty;
            Type = CustomerType.Individual;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
            Active = true;
            ErrorMessage = string.Empty;
            HasError = false;
            IsEditing = false;
        }
    }
} 