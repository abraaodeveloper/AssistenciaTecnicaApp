using ReactiveUI;
using System;
using Avalonia.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using AssistenciaTecnicaApp.Views;

namespace AssistenciaTecnicaApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email = string.Empty;
        private string _senha = string.Empty;

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

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    // Abre a janela principal
                    var mainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel()
                    };
                    mainWindow.Show();

                    // Fecha a janela de login
                    if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var loginWindow = desktop.MainWindow;
                        desktop.MainWindow = mainWindow;
                        loginWindow?.Close();
                    }
                });
            });
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
} 