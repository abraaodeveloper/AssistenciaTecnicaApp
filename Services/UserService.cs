using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AssistenciaTecnicaApp.Data;
using AssistenciaTecnicaApp.Models;
using Serilog;

namespace AssistenciaTecnicaApp.Services
{
    /// <summary>
    /// Interface for user management operations.
    /// </summary>
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string senha);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }

    /// <summary>
    /// Implementation of user management service including authentication,
    /// user creation, updating, and deletion.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Authenticates a user based on email and password.
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="senha">User's password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public async Task<User?> AuthenticateAsync(string email, string senha)
        {
            try
            {
                Log.Information("Authentication attempt for email: {Email}", email);
                
                // Check if database is accessible
                if (!await _context.Database.CanConnectAsync())
                {
                    Log.Error("Could not connect to database during authentication");
                    throw new Exception("Could not connect to database");
                }
                
                // Check if users exist
                if (!await _context.Users.AnyAsync())
                {
                    Log.Warning("No users found in database during authentication");
                }
                
                // In production, implement secure password comparison with hashing
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.Email.ToLower() == email.ToLower() && u.Senha == senha);
                
                if (user != null)
                {
                    Log.Information("Authentication successful for: {Email}", email);
                    return user;
                }
                
                Log.Warning("Authentication failed for: {Email} - Invalid credentials", email);
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during authentication for email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Gets all users from the database.
        /// </summary>
        /// <returns>Collection of all users</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User object if found, null otherwise</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">User object to create</param>
        /// <returns>True if creation successful, false otherwise</returns>
        public async Task<bool> CreateUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">User object with updated values</param>
        /// <returns>True if update successful, false otherwise</returns>
        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            // Update properties
            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="id">ID of the user to delete</param>
        /// <returns>True if deletion successful, false otherwise</returns>
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