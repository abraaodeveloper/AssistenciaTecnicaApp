using Microsoft.EntityFrameworkCore;
using AssistenciaTecnicaApp.Models;

namespace AssistenciaTecnicaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Adicione DbSets para suas entidades aqui
        public DbSet<User> Users { get; set; }
        
        // O método será expandido quando tivermos mais entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure o modelo conforme necessário
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Senha).IsRequired();
                
                // Seed de dados iniciais
                entity.HasData(
                    new User { Id = 1, Nome = "Administrador", Email = "admin@example.com", Senha = "admin123", Cargo = UserRole.Admin, LojaId = 1 }
                );
            });
        }
    }
} 