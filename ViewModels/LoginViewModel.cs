using ReactiveUI;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using System.Diagnostics;
using AssistenciaTecnicaApp.Views;
using AssistenciaTecnicaApp.Services;
using AssistenciaTecnicaApp.Models;
using Serilog;

namespace AssistenciaTecnicaApp.ViewModels
{
    /// <summary>
    /// ViewModel for the login screen that handles user authentication.
    /// </summary>
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
                // Clear previous error message
                ErrorMessage = string.Empty;
                
                // Validate inputs
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
                {
                    ErrorMessage = "Please fill in all fields.";
                    return;
                }
                
                // Start loading state
                IsLoading = true;
                
                Log.Information("Starting login process for: {Email}", Email);
                
                // Authenticate user
                try
                {
                    var user = await _userService.AuthenticateAsync(Email, Senha);
                    
                    if (user != null)
                    {
                        // User authenticated successfully
                        Log.Information("User authenticated successfully: {Email} - ID: {Id}, Name: {Nome}, Role: {Cargo}", 
                            Email, user.Id, user.Nome, user.Cargo);
                        
                        // Open main window on UI thread
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            try
                            {
                                Log.Debug("Creating main window instance");
                                var mainWindow = new MainWindow
                                {
                                    DataContext = new MainWindowViewModel { CurrentUser = user }
                                };
                                Log.Debug("Displaying main window");
                                mainWindow.Show();
    
                                // Close login window
                                if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                                {
                                    var loginWindow = desktop.MainWindow;
                                    desktop.MainWindow = mainWindow;
                                    Log.Debug("Closing login window");
                                    loginWindow?.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Error opening main window. Details: {Details}", ex.ToString());
                                ErrorMessage = "Error opening main window. Check logs for details.";
                                IsLoading = false;
                            }
                        });
                    }
                    else
                    {
                        // Authentication failed
                        Log.Warning("Authentication failed for: {Email}", Email);
                        ErrorMessage = "Invalid email or password.";
                    }
                }
                catch (Exception ex) when (ex.Message.Contains("banco de dados") || ex.Message.Contains("database"))
                {
                    Log.Error(ex, "Database error during authentication");
                    ErrorMessage = "Could not access the database. Check your connection.";
                }
                catch (Exception ex)
                {
                    // Other type of error during authentication
                    Log.Error(ex, "Error during authentication");
                    ErrorMessage = "Authentication error. Check logs for details.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in login process");
                ErrorMessage = "An error occurred while trying to log in. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// Command that supports asynchronous operations for the MVVM pattern.
    /// </summary>
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