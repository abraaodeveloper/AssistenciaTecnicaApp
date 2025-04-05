using Microsoft.EntityFrameworkCore;
using AssistenciaTecnicaApp.Models;
using System;

namespace AssistenciaTecnicaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Document).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.ZipCode).HasMaxLength(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Senha).IsRequired().HasMaxLength(100);
            });
            
            // Seed data for testing
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Nome = "Administrator", 
                    Email = "admin@example.com", 
                    Senha = "admin123", 
                    Cargo = UserRole.Admin,
                    LojaId = 1
                }
            );
            
            // Seed sample customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer
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
                    Active = true,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Customer
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
                    Active = true,
                    CreatedAt = DateTime.Now.AddDays(-15)
                }
            );
        }
    }
} 