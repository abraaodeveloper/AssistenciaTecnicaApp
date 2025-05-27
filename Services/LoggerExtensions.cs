using Microsoft.Extensions.DependencyInjection;

namespace AssistenciaTecnicaApp.Services
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddAppLogging(this IServiceCollection services)
        {
            // Registra o logger como singleton para toda a aplicação
            services.AddSingleton<ILogger>(_ => new AvaloniaLogger());
            return services;
        }
    }
} 