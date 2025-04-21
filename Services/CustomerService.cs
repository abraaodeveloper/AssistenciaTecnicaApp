using AssistenciaTecnicaApp.Data;
using AssistenciaTecnicaApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace AssistenciaTecnicaApp.Services
{
    /// <summary>
    /// Service for managing customer data operations
    /// </summary>
    public class CustomerService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        
        public CustomerService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }
        
        /// <summary>
        /// Retrieves all customers from the database
        /// </summary>
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            try
            {
                Log.Debug("[CustomerService] Getting all customers");
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var customers = await context.Customers
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                Log.Debug("[CustomerService] Retrieved {Count} customers", customers.Count);
                return customers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Error retrieving all customers");
                throw new ApplicationException("Failed to retrieve customers", ex);
            }
        }
        
        /// <summary>
        /// Gets a customer by ID
        /// </summary>
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            try
            {
                Log.Debug("[CustomerService] Getting customer by ID: {Id}", id);
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var customer = await context.Customers.FindAsync(id);
                
                if (customer == null)
                {
                    Log.Warning("[CustomerService] Customer not found with ID: {Id}", id);
                }
                else
                {
                    Log.Debug("[CustomerService] Retrieved customer: {Name}, ID: {Id}", customer.Name, customer.Id);
                }
                
                return customer;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Error retrieving customer with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve customer with ID {id}", ex);
            }
        }
        
        /// <summary>
        /// Adds a new customer to the database
        /// </summary>
        public async Task<int> AddCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                Log.Error("[CustomerService] Null customer object passed to AddCustomerAsync");
                throw new ArgumentNullException(nameof(customer));
            }
            
            try
            {
                Log.Debug("[CustomerService] Starting to add new customer: {Name}, Type: {Type}", 
                    customer.Name, customer.Type);
                
                // Logging details before database operation
                Log.Debug("[CustomerService] Customer details: Document: {Document}, Phone: {Phone}, Email: {Email}", 
                    customer.Document, customer.Phone, customer.Email);
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Add to DbContext
                Log.Debug("[CustomerService] Adding customer to DbContext");
                context.Customers.Add(customer);
                
                // Save changes
                Log.Debug("[CustomerService] Saving changes to database");
                var affectedRows = await context.SaveChangesAsync();
                Log.Debug("[CustomerService] SaveChangesAsync completed with {Count} affected rows", affectedRows);
                
                Log.Information("[CustomerService] Added new customer successfully. ID: {Id}, Name: {Name}", 
                    customer.Id, customer.Name);
                
                return customer.Id;
            }
            catch (DbUpdateException dbEx)
            {
                Log.Error(dbEx, "[CustomerService] Database update error adding customer: {Name}", customer.Name);
                if (dbEx.InnerException != null)
                {
                    Log.Error(dbEx.InnerException, "[CustomerService] Inner exception details: {Message}", 
                        dbEx.InnerException.Message);
                }
                throw new ApplicationException("Failed to add customer due to database error", dbEx);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Unexpected error adding new customer: {Name}", customer.Name);
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "[CustomerService] Inner exception details: {Message}", 
                        ex.InnerException.Message);
                }
                throw new ApplicationException("Failed to add customer", ex);
            }
        }
        
        /// <summary>
        /// Updates an existing customer
        /// </summary>
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }
            
            try
            {
                Log.Debug("[CustomerService] Updating customer ID: {Id}, Name: {Name}", 
                    customer.Id, customer.Name);
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Check if customer exists
                var existingCustomer = await context.Customers.FindAsync(customer.Id);
                
                if (existingCustomer == null)
                {
                    Log.Warning("[CustomerService] Customer not found for update. ID: {Id}", customer.Id);
                    return false;
                }
                
                // Update properties
                existingCustomer.Name = customer.Name;
                existingCustomer.Document = customer.Document;
                existingCustomer.Type = customer.Type;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.Email = customer.Email;
                existingCustomer.Address = customer.Address;
                existingCustomer.City = customer.City;
                existingCustomer.State = customer.State;
                existingCustomer.ZipCode = customer.ZipCode;
                existingCustomer.Active = customer.Active;
                existingCustomer.UpdatedAt = DateTime.Now;
                
                await context.SaveChangesAsync();
                
                Log.Information("[CustomerService] Updated customer successfully. ID: {Id}", customer.Id);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Error updating customer ID: {Id}", customer.Id);
                throw new ApplicationException($"Failed to update customer with ID {customer.Id}", ex);
            }
        }
        
        /// <summary>
        /// Deletes a customer by ID
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                Log.Debug("[CustomerService] Deleting customer ID: {Id}", id);
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var customer = await context.Customers.FindAsync(id);
                
                if (customer == null)
                {
                    Log.Warning("[CustomerService] Customer not found for deletion. ID: {Id}", id);
                    return false;
                }
                
                context.Customers.Remove(customer);
                await context.SaveChangesAsync();
                
                Log.Information("[CustomerService] Deleted customer successfully. ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Error deleting customer ID: {Id}", id);
                throw new ApplicationException($"Failed to delete customer with ID {id}", ex);
            }
        }
        
        /// <summary>
        /// Searches for customers based on a search term
        /// </summary>
        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCustomersAsync();
            }
            
            try
            {
                Log.Debug("[CustomerService] Searching customers with term: {Term}", searchTerm);
                
                // Criar um novo contexto para esta operação
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var normalizedTerm = searchTerm.ToLower();
                
                var customers = await context.Customers
                    .Where(c => 
                        c.Name.ToLower().Contains(normalizedTerm) ||
                        c.Document.ToLower().Contains(normalizedTerm) ||
                        c.Email.ToLower().Contains(normalizedTerm) ||
                        c.Phone.Contains(normalizedTerm) ||
                        c.City.ToLower().Contains(normalizedTerm))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                Log.Debug("[CustomerService] Search returned {Count} customers", customers.Count);
                return customers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomerService] Error searching customers with term: {Term}", searchTerm);
                throw new ApplicationException("Failed to search customers", ex);
            }
        }
    }
} 