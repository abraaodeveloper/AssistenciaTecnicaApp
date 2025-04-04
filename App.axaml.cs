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

namespace AssistenciaTecnicaApp;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configurar o logger
            ConfigureLogging();
            
            // Configurar os serviços
            Log.Debug("Configurando serviços...");
            ConfigureServices();
            Log.Debug("Serviços configurados com sucesso");

            // Desativar validação de dados
            Log.Debug("Desativando validação de dados...");
            DisableAvaloniaDataAnnotationValidation();
            Log.Debug("Validação de dados desativada");
            
            // Configurar a janela principal
            Log.Debug("Criando janela de login...");
            desktop.MainWindow = new LoginWindow
            {
                DataContext = new LoginViewModel(ServiceProvider!.GetRequiredService<IUserService>())
            };
            Log.Debug("Janela de login criada com sucesso");
            
            // Inicializar o banco de dados
            Log.Debug("Inicializando banco de dados...");
            InitializeDatabase();
            Log.Debug("Inicialização concluída");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Configurar o contexto do banco de dados
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "AssistenciaTecnicaApp", "assistencia.db");
        
        // Garantir que o diretório existe
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // Adicionar serviços
        services.AddTransient<IUserService, UserService>();
        services.AddSingleton<ThemeService>(_ => new ThemeService(this));
        
        // Construir provedor de serviços
        ServiceProvider = services.BuildServiceProvider();
    }
    
    private void ConfigureLogging()
    {
        // Configurar o logger global
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AssistenciaTecnicaApp", "logs", "app.log");
            
        // Garantir que o diretório de logs existe
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
            
        // Em ambiente de desenvolvimento, também escrever logs no console
#if DEBUG
        logConfig = logConfig.WriteTo.Console();
#endif
            
        Log.Logger = logConfig.CreateLogger();
            
        // Registrar início da aplicação
        Log.Information("Aplicação iniciada");
    }
    
    private void InitializeDatabase()
    {
        try
        {
            using var scope = ServiceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Aplicar migrações pendentes e criar o banco se não existir
            context.Database.EnsureCreated();
            
            // Verificar se já existem usuários
            if (!context.Users.Any())
            {
                // Adicionar um usuário administrador padrão
                context.Users.Add(new User { 
                    Id = 1, 
                    Nome = "Administrador", 
                    Email = "admin@example.com", 
                    Senha = "admin123", 
                    Cargo = UserRole.Admin, 
                    LojaId = 1 
                });
                context.SaveChanges();
                Log.Information("Usuário administrador padrão criado");
            }
            
            // Verificar se o usuário admin existe
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
            if (adminUser != null)
            {
                Log.Information("Usuário admin encontrado: {0}", adminUser.Nome);
            }
            else
            {
                Log.Warning("Usuário admin não encontrado no banco de dados");
            }
            
            Log.Information("Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao inicializar o banco de dados");
        }
    }

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