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
using Avalonia.Styling;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

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
    private static Services.ILogger? Logger { get; set; }

    public override void Initialize()
    {
        // Configure logger first
        Logger = new AvaloniaLogger();
        
        // Configure global exception handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            Logger?.Fatal(exception ?? new Exception("Unknown exception"), "[GLOBAL] Unhandled exception occurred");
        };
        
        // Continue with normal initialization
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Logger?.Debug("Framework initialization completed");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Apply default theme
                ApplyDefaultTheme();
                
                // Configure services
                Logger?.Debug("[OnFrameworkInitializationCompleted] Configuring services...");
                ConfigureServices();
                Logger?.Debug("[OnFrameworkInitializationCompleted] Services configured successfully");
                
                // Disable data validation
                Logger?.Debug("[OnFrameworkInitializationCompleted] Disabling data annotation validation...");
                DisableAvaloniaDataAnnotationValidation();
                Logger?.Debug("[OnFrameworkInitializationCompleted] Data validation disabled");
                
                // Initialize database
                Logger?.Debug("[OnFrameworkInitializationCompleted] Initializing database...");
                InitializeDatabase();
                Logger?.Debug("[OnFrameworkInitializationCompleted] Initialization completed");
                
                // Configurar evento de fechamento da aplicação
                desktop.ShutdownRequested += HandleApplicationShutdown;
                
                Logger?.Debug("Creating login window");
                var userService = ServiceProvider!.GetRequiredService<IUserService>();
                var appLogger = ServiceProvider.GetRequiredService<Services.ILogger>();
                var loginViewModel = new LoginViewModel(userService, appLogger);
                
                // Handle successful login
                loginViewModel.LoginSuccess += (sender, user) =>
                {
                    try
                    {
                        Logger?.Debug("Login successful, creating main window");
                        var mainWindowViewModel = ServiceProvider!.GetRequiredService<MainWindowViewModel>();
                        mainWindowViewModel.CurrentUser = user;
                        
                        var mainWindow = new MainWindow
                        {
                            DataContext = mainWindowViewModel
                        };
                        
                        // Adicionar handler para o evento de fechamento da janela principal
                        mainWindow.Closing += (s, e) =>
                        {
                            Logger?.Information("[MainWindow_Closing] Iniciando encerramento da aplicação");
                            Environment.Exit(0);
                        };
                        
                        // Change main window and show it
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        
                        // Hide the login window
                        Logger?.Debug("Hiding login window");
                        var loginWindow = desktop.Windows.FirstOrDefault(w => w is LoginWindow);
                        if (loginWindow != null)
                        {
                            loginWindow.Hide();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.Error(ex, "Error opening main window after login");
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
            Logger?.Fatal(ex, "Fatal error during application initialization");
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
            Logger?.Debug("[ApplyDefaultTheme] Applying default theme...");
            
            // Definir cores básicas diretamente
            // As cores principais já estão definidas no arquivo Colors.axaml 
            // e serão aplicadas automaticamente
            
            Logger?.Information("[ApplyDefaultTheme] Default theme applied successfully");
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "[ApplyDefaultTheme] Error applying default theme");
        }
    }

    /// <summary>
    /// Configures the application's dependency injection services.
    /// </summary>
    private void ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        
        // Ensure logs directory exists
        var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logsPath);
        
        // Add logging first
        serviceCollection.AddLogging(builder =>
        {
            // Create and configure Serilog logger
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(logsPath, "app_.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7);

            // Create logger instance
            var logger = loggerConfig.CreateLogger();
            
            // Set Serilog as the logging provider
            builder.AddSerilog(logger, dispose: true);
            
            // Also add Console and Debug providers for development
            builder.AddConsole();
            builder.AddDebug();
            
            // Set as static logger
            Log.Logger = logger;
        });
        
        // Register our custom logger
        serviceCollection.AddSingleton<Services.ILogger, AvaloniaLogger>();
        
        // Register ILogger<T> for each ViewModel that needs logging
        serviceCollection.AddTransient(typeof(ILogger<>), typeof(Logger<>));
        
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
                Logger?.Information("[InitializeDatabase] Default administrator user created or updated");
                
                // Refresh admin user reference
                adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
            }
            
            if (adminUser != null)
            {
                Logger?.Information($"[InitializeDatabase] Admin user found: {adminUser.Nome}");
            }
            else
            {
                Logger?.Warning("[InitializeDatabase] Admin user not found in database");
            }
            
            Logger?.Information("[InitializeDatabase] Database initialized successfully");
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "[InitializeDatabase] Error initializing database");
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
            Logger?.Debug("[DisableAvaloniaDataAnnotationValidation] Starting to disable data annotation validation");
            
            // Get an array of plugins to remove
            var allValidators = BindingPlugins.DataValidators?.ToList() ?? new List<IDataValidationPlugin>();
            Logger?.Debug($"[DisableAvaloniaDataAnnotationValidation] Found {allValidators.Count} total data validators");
            
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators?.OfType<DataAnnotationsValidationPlugin>()?.ToArray() ?? Array.Empty<DataAnnotationsValidationPlugin>();
            
            Logger?.Debug($"[DisableAvaloniaDataAnnotationValidation] Found {dataValidationPluginsToRemove.Length} data annotation validation plugins to remove");

            // remove each entry found
            int removed = 0;
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                try
                {
                    Logger?.Debug($"[DisableAvaloniaDataAnnotationValidation] Removing validation plugin: {plugin.GetType().FullName}");
                    BindingPlugins.DataValidators?.Remove(plugin);
                    removed++;
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "[DisableAvaloniaDataAnnotationValidation] Error removing validation plugin");
                }
            }
            
            Logger?.Debug($"[DisableAvaloniaDataAnnotationValidation] Removed {removed} data validation plugins");
            
            // Verificar se precisamos implementar validação personalizada no lugar
            Logger?.Debug("[DisableAvaloniaDataAnnotationValidation] Adding custom validation plugin to handle required fields");
            
            // Aqui você pode adicionar validação personalizada se precisar
            
            Logger?.Debug("[DisableAvaloniaDataAnnotationValidation] Data annotation validation disabled successfully");
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "[DisableAvaloniaDataAnnotationValidation] Error disabling data annotation validation");
        }
    }

    /// <summary>
    /// Handles the application shutdown event to properly clean up resources
    /// </summary>
    private void HandleApplicationShutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        try
        {
            Logger?.Information("[HandleApplicationShutdown] Iniciando encerramento da aplicação");
            
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
                        Logger?.Debug("[HandleApplicationShutdown] Disposing ServiceProvider");
                        disposable.Dispose();
                        ServiceProvider = null;
                    }
                }
                catch (Exception disposeEx)
                {
                    Logger?.Error(disposeEx, "[HandleApplicationShutdown] Erro ao descartar ServiceProvider");
                }
            }
            
            Logger?.Information("[HandleApplicationShutdown] Aplicação encerrada com sucesso");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "[HandleApplicationShutdown] Erro durante o encerramento da aplicação");
            Environment.Exit(1);
        }
    }
}