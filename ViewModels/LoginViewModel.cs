using ReactiveUI;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using AssistenciaTecnicaApp.Views;
using AssistenciaTecnicaApp.Services;
using AssistenciaTecnicaApp.Models;
using Serilog;

namespace AssistenciaTecnicaApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Senha
        {
            get => _senha;
            set => this.RaiseAndSetIfChanged(ref _senha, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IUserService userService)
        {
            _userService = userService;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }
        
        private async Task LoginAsync(object? parameter)
        {
            try
            {
                // Limpar mensagem de erro anterior
                ErrorMessage = string.Empty;
                
                // Validar entradas
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
                {
                    ErrorMessage = "Por favor, preencha todos os campos.";
                    return;
                }
                
                // Iniciar carregamento
                IsLoading = true;
                
                Log.Information("Iniciando processo de login para: {Email}", Email);
                
                // Autenticar usuário
                try
                {
                    var user = await _userService.AuthenticateAsync(Email, Senha);
                    
                    if (user != null)
                    {
                        // Usuário autenticado com sucesso
                        Log.Information("Usuário autenticado com sucesso: {Email}", Email);
                        
                        // Abrir a janela principal no thread UI
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            try
                            {
                                var mainWindow = new MainWindow
                                {
                                    DataContext = new MainWindowViewModel { CurrentUser = user }
                                };
                                mainWindow.Show();
    
                                // Fecha a janela de login
                                if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                                {
                                    var loginWindow = desktop.MainWindow;
                                    desktop.MainWindow = mainWindow;
                                    loginWindow?.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Erro ao abrir a janela principal");
                                ErrorMessage = "Erro ao abrir a janela principal. Verifique os logs.";
                                IsLoading = false;
                            }
                        });
                    }
                    else
                    {
                        // Falha na autenticação
                        Log.Warning("Falha na autenticação para: {Email}", Email);
                        ErrorMessage = "Email ou senha inválidos.";
                    }
                }
                catch (Exception ex) when (ex.Message.Contains("banco de dados"))
                {
                    Log.Error(ex, "Erro de banco de dados durante autenticação");
                    ErrorMessage = "Não foi possível acessar o banco de dados. Verifique sua conexão.";
                }
                catch (Exception ex)
                {
                    // Outro tipo de erro durante autenticação
                    Log.Error(ex, "Erro durante autenticação");
                    ErrorMessage = "Erro de autenticação. Verifique os logs.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro inesperado no processo de login");
                ErrorMessage = "Ocorreu um erro ao tentar fazer login. Tente novamente.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private bool _isExecuting = false;

        public AsyncRelayCommand(Func<object?, Task> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => !_isExecuting;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 