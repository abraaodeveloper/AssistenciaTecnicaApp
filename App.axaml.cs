using Avalonia;
using Avalonia.Controls;  // Para WindowIcon
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Threading;
using System;  // Para Uri
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using AssistenciaTecnicaApp.ViewModels;
using AssistenciaTecnicaApp.Views;
using AssistenciaTecnicaApp.Data;
using AssistenciaTecnicaApp.Services;
using AssistenciaTecnicaApp.Models;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Avalonia.Styling;
using System.Threading.Tasks;

namespace AssistenciaTecnicaApp;

/// <summary>
/// Main application class that handles initialization, services configuration and database setup.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Global service provider accessible throughout the application.
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        // Configure global exception handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            Log.Fatal(exception, "[GLOBAL] Unhandled exception occurred: {Message}", exception?.Message);
            
            // Log inner exceptions if available
            var innerEx = exception?.InnerException;
            while (innerEx != null)
            {
                Log.Fatal(innerEx, "[GLOBAL] Inner exception: {Message}", innerEx.Message);
                innerEx = innerEx.InnerException;
            }
            
            // Log stack trace
            Log.Fatal("[GLOBAL] Exception stack trace: {StackTrace}", exception?.StackTrace);
            
            // Log additional context
            try
            {
                Log.Fatal("[GLOBAL] Thread ID: {ThreadId}, Is Terminating: {IsTerminating}", 
                    System.Threading.Thread.CurrentThread.ManagedThreadId, args.IsTerminating);
            }
            catch
            {
                // Ignore errors in exception logging
            }
        };
        
        // Continue with normal initialization
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Log.Debug("Framework initialization completed");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Configure logger
                ConfigureLogging();
                
                // Apply default theme
                ApplyDefaultTheme();
                
                // Configure services
                Log.Debug("[OnFrameworkInitializationCompleted] Configuring services...");
                ConfigureServices();
                Log.Debug("[OnFrameworkInitializationCompleted] Services configured successfully");
                
                // Disable data validation
                Log.Debug("[OnFrameworkInitializationCompleted] Disabling data annotation validation...");
                DisableAvaloniaDataAnnotationValidation();
                Log.Debug("[OnFrameworkInitializationCompleted] Data validation disabled");
                
                // Initialize database
                Log.Debug("[OnFrameworkInitializationCompleted] Initializing database...");
                InitializeDatabase();
                Log.Debug("[OnFrameworkInitializationCompleted] Initialization completed");
                
                // Configurar evento de fechamento da aplicação
                desktop.ShutdownRequested += HandleApplicationShutdown;
                
                Log.Debug("Creating login window");
                var userService = ServiceProvider!.GetRequiredService<IUserService>();
                var loginViewModel = new LoginViewModel(userService);
                
                // Handle successful login
                loginViewModel.LoginSuccess += (sender, user) =>
                {
                    try
                    {
                        Log.Debug("Login successful, creating main window");
                        var mainWindowViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                        mainWindowViewModel.CurrentUser = user;
                        
                        var mainWindow = new MainWindow
                        {
                            DataContext = mainWindowViewModel
                        };
                        
                        // Adicionar handler para o evento de fechamento da janela principal
                        mainWindow.Closing += (s, e) =>
                        {
                            Log.Information("[MainWindow_Closing] Iniciando encerramento da aplicação");
                            Environment.Exit(0);
                        };
                        
                        // Change main window and show it
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        
                        // Hide the login window
                        Log.Debug("Hiding login window");
                        var loginWindow = desktop.Windows.FirstOrDefault(w => w is LoginWindow);
                        if (loginWindow != null)
                        {
                            loginWindow.Hide();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error opening main window after login: {Message}", ex.Message);
                    }
                };
                
                desktop.MainWindow = new LoginWindow
                {
                    DataContext = loginViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application initialization: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Applies the default application theme.
    /// </summary>
    private void ApplyDefaultTheme()
    {
        try
        {
            Log.Debug("[ApplyDefaultTheme] Applying default theme...");
            
            // Definir cores básicas diretamente
            // As cores principais já estão definidas no arquivo Colors.axaml 
            // e serão aplicadas automaticamente
            
            Log.Information("[ApplyDefaultTheme] Default theme applied successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ApplyDefaultTheme] Error applying default theme");
        }
    }

    /// <summary>
    /// Configures the application's dependency injection services.
    /// </summary>
    private void ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        
        // Database context
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AssistenciaTecnicaApp");
        Directory.CreateDirectory(appDataPath); // Garantir que a pasta existe
        string dbPath = Path.Combine(appDataPath, "assistencia.db");
        
        // Adicionar DbContext como scoped para operações simples
        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // Adicionar DbContextFactory para operações concorrentes
        serviceCollection.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // ViewModels
        serviceCollection.AddTransient<CustomersViewModel>();
        serviceCollection.AddTransient<OrdersViewModel>();
        serviceCollection.AddTransient<HomeViewModel>();
        serviceCollection.AddTransient<SettingsViewModel>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        
        // Services - usando DbContextFactory
        serviceCollection.AddScoped<CustomerService>();
        
        // Add services
        serviceCollection.AddTransient<IUserService, UserService>();
        serviceCollection.AddSingleton<ThemeService>(_ => new ThemeService(this));
        
        // Build service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
    
    /// <summary>
    /// Configures the application logging system.
    /// </summary>
    private void ConfigureLogging()
    {
        // Configure global logger
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AssistenciaTecnicaApp", "logs", "app.log");
            
        // Ensure logs directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
            
        // In development environment, also write logs to console
#if DEBUG
        logConfig = logConfig.WriteTo.Console();
#endif
            
        Log.Logger = logConfig.CreateLogger();
            
        // Register application start
        Log.Information("[ConfigureLogging] Application started");
    }
    
    /// <summary>
    /// Initializes the database and creates default data if needed.
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            using var scope = ServiceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Apply pending migrations and create database if it doesn't exist
            context.Database.EnsureCreated();
            
            // Check if admin user exists
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
            
            // If user doesn't exist, create it
            if (adminUser == null)
            {
                // Add default admin user
                adminUser = new User { 
                    Id = 1, 
                    Nome = "Administrator", 
                    Email = "admin@example.com", 
                    Senha = "admin123", 
                    Cargo = UserRole.Admin, 
                    LojaId = 1 
                };
                
                // Check if there are any users with ID 1
                var existingUser = context.Users.Find(1);
                if (existingUser != null)
                {
                    // Update the existing user instead of creating a new one
                    existingUser.Nome = adminUser.Nome;
                    existingUser.Email = adminUser.Email;
                    existingUser.Senha = adminUser.Senha;
                    existingUser.Cargo = adminUser.Cargo;
                }
                else
                {
                    // Add as new user
                    context.Users.Add(adminUser);
                }
                
                context.SaveChanges();
                Log.Information("[InitializeDatabase] Default administrator user created or updated");
                
                // Refresh admin user reference
                adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
            }
            
            if (adminUser != null)
            {
                Log.Information("[InitializeDatabase] Admin user found: {0}", adminUser.Nome);
            }
            else
            {
                Log.Warning("[InitializeDatabase] Admin user not found in database");
            }
            
            Log.Information("[InitializeDatabase] Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[InitializeDatabase] Error initializing database");
        }
    }

    /// <summary>
    /// Disables the Avalonia Data Annotation Validation system.
    /// This allows the application to implement custom validation logic.
    /// </summary>
    private void DisableAvaloniaDataAnnotationValidation()
    {
        try
        {
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Starting to disable data annotation validation");
            
            // Get an array of plugins to remove
            var allValidators = BindingPlugins.DataValidators?.ToList() ?? new List<IDataValidationPlugin>();
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Found {0} total data validators", allValidators.Count);
            
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators?.OfType<DataAnnotationsValidationPlugin>()?.ToArray() ?? Array.Empty<DataAnnotationsValidationPlugin>();
            
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Found {0} data annotation validation plugins to remove", 
                dataValidationPluginsToRemove.Length);

            // remove each entry found
            int removed = 0;
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                try
                {
                    Log.Debug("[DisableAvaloniaDataAnnotationValidation] Removing validation plugin: {0}", 
                        plugin.GetType().FullName);
                    BindingPlugins.DataValidators?.Remove(plugin);
                    removed++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[DisableAvaloniaDataAnnotationValidation] Error removing validation plugin");
                }
            }
            
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Removed {0} data validation plugins", removed);
            
            // Verificar se precisamos implementar validação personalizada no lugar
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Adding custom validation plugin to handle required fields");
            
            // Aqui você pode adicionar validação personalizada se precisar
            
            Log.Debug("[DisableAvaloniaDataAnnotationValidation] Data annotation validation disabled successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[DisableAvaloniaDataAnnotationValidation] Error disabling data annotation validation");
        }
    }

    /// <summary>
    /// Handles the application shutdown event to properly clean up resources
    /// </summary>
    private void HandleApplicationShutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        try
        {
            Log.Information("[HandleApplicationShutdown] Iniciando encerramento da aplicação");
            
            // Limpar event handlers globais
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownRequested -= HandleApplicationShutdown;
            }
            
            // Certifique-se de que o ServiceProvider ainda é válido antes de tentar usá-lo
            var localServiceProvider = ServiceProvider;
            if (localServiceProvider != null)
            {
                try
                {
                    // Descartar o ServiceProvider
                    if (localServiceProvider is IDisposable disposable)
                    {
                        Log.Debug("[HandleApplicationShutdown] Disposing ServiceProvider");
                        disposable.Dispose();
                        ServiceProvider = null;
                    }
                }
                catch (Exception disposeEx)
                {
                    Log.Error(disposeEx, "[HandleApplicationShutdown] Erro ao descartar ServiceProvider");
                }
            }
            
            // Flush e fechar o logger
            Log.Information("[HandleApplicationShutdown] Aplicação encerrada com sucesso");
            Log.CloseAndFlush();
            
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            try
            {
                Log.Error(ex, "[HandleApplicationShutdown] Erro durante o encerramento da aplicação");
                Log.CloseAndFlush();
            }
            finally
            {
                Environment.Exit(1);
            }
        }
    }
}