using Avalonia;
using Avalonia.Controls;  // Para WindowIcon
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;  // Para Uri
using System.Linq;
using System.IO;
using System.Diagnostics;
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
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
            
            // Configure main window
            Log.Debug("[OnFrameworkInitializationCompleted] Creating login window...");
            desktop.MainWindow = new LoginWindow
            {
                DataContext = new LoginViewModel(ServiceProvider!.GetRequiredService<IUserService>())
            };
            Log.Debug("[OnFrameworkInitializationCompleted] Login window created successfully");
            
            // Initialize database
            Log.Debug("[OnFrameworkInitializationCompleted] Initializing database...");
            InitializeDatabase();
            Log.Debug("[OnFrameworkInitializationCompleted] Initialization completed");
        }

        base.OnFrameworkInitializationCompleted();
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
        var services = new ServiceCollection();
        
        // Configure database context
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AssistenciaTecnicaApp", "assistencia.db");
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // Add services
        services.AddTransient<IUserService, UserService>();
        services.AddSingleton<ThemeService>(_ => new ThemeService(this));
        
        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
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
            
            // Check if users already exist
            if (!context.Users.Any())
            {
                // Add default admin user
                context.Users.Add(new User { 
                    Id = 1, 
                    Nome = "Administrator", 
                    Email = "admin@example.com", 
                    Senha = "admin123", 
                    Cargo = UserRole.Admin, 
                    LojaId = 1 
                });
                context.SaveChanges();
                Log.Information("[InitializeDatabase] Default administrator user created");
            }
            
            // Check if admin user exists
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
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
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}