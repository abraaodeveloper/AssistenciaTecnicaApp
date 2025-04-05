using AssistenciaTecnicaApp.Data;
using AssistenciaTecnicaApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssistenciaTecnicaApp.Services
{
    public class CustomerService
    {
        private readonly ApplicationDbContext _context;
        
        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }
        
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }
        
        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();
                
            searchTerm = searchTerm.ToLower();
            
            return await _context.Customers
                .Where(c => c.Name.ToLower().Contains(searchTerm) ||
                           c.Document.ToLower().Contains(searchTerm) ||
                           c.Email.ToLower().Contains(searchTerm) ||
                           c.Phone.Contains(searchTerm))
                .ToListAsync();
        }
        
        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.Now;
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            return customer;
        }
        
        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.Id);
            
            if (existingCustomer == null)
                throw new KeyNotFoundException($"Customer with ID {customer.Id} not found");
                
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
            
            await _context.SaveChangesAsync();
            
            return existingCustomer;
        }
        
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            if (customer == null)
                return false;
                
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
} 