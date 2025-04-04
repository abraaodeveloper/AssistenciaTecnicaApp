using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AssistenciaTecnicaApp.Data;
using AssistenciaTecnicaApp.Models;
using Serilog;

namespace AssistenciaTecnicaApp.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string senha);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string email, string senha)
        {
            try
            {
                Log.Information("Tentativa de autenticação para o email: {Email}", email);
                Trace.WriteLine($"Tentativa de autenticação para o email: {email}");
                
                // Verificar se o banco de dados está acessível
                if (!await _context.Database.CanConnectAsync())
                {
                    Log.Error("Não foi possível conectar ao banco de dados durante a autenticação");
                    Trace.WriteLine("ERRO: Não foi possível conectar ao banco de dados durante a autenticação");
                    throw new Exception("Não foi possível conectar ao banco de dados");
                }
                
                // Verificar se existem usuários
                if (!await _context.Users.AnyAsync())
                {
                    Log.Warning("Nenhum usuário encontrado no banco de dados durante a autenticação");
                    Trace.WriteLine("ALERTA: Nenhum usuário encontrado no banco de dados");
                }
                
                // Na produção, deve-se implementar uma comparação segura de senhas com hashing
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.Email.ToLower() == email.ToLower() && u.Senha == senha);
                
                if (user != null)
                {
                    Log.Information("Autenticação bem-sucedida para: {Email}", email);
                    Trace.WriteLine($"Autenticação bem-sucedida para: {email}");
                    return user;
                }
                
                Log.Warning("Autenticação falhou para: {Email} - Credenciais inválidas", email);
                Trace.WriteLine($"Autenticação falhou para: {email} - Credenciais inválidas");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro durante autenticação para o email: {Email}", email);
                Trace.WriteLine($"ERRO durante autenticação: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            // Atualiza as propriedades
            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 