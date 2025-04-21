using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using Serilog;
using System;
using System.Threading.Tasks;

namespace AssistenciaTecnicaApp.Views
{
    public partial class CustomersView : UserControl
    {
        // Flag para evitar operações duplicadas
        private bool _isProcessingTextChange = false;
        
        public CustomersView()
        {
            try
            {
                Log.Debug("[CustomersView] Iniciando construção da view");
                
                // Registrar para receber notificações de possíveis exceções não tratadas
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    var ex = args.ExceptionObject as Exception;
                    Log.Fatal(ex, "[CustomersView] Exceção não tratada no domínio da aplicação: {Message}", ex?.Message);
                    
                    // Capturar informações adicionais sobre a thread
                    Log.Fatal("[CustomersView] Thread ID: {ThreadId}, Is UI Thread: {IsUIThread}", 
                        System.Threading.Thread.CurrentThread.ManagedThreadId,
                        Dispatcher.UIThread.CheckAccess());
                    
                    // Se possível, mostrar um diálogo de erro - mas fazer isso na thread da UI
                    Dispatcher.UIThread.Post(() => 
                    {
                        try
                        {
                            var dialog = new Avalonia.Controls.Window
                            {
                                Title = "Erro na Aplicação",
                                Content = new TextBlock { 
                                    Text = $"Ocorreu um erro não tratado: {ex?.Message}\n\nA aplicação pode estar instável. Considere reiniciá-la.",
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                    Margin = new Thickness(20)
                                },
                                Width = 400,
                                Height = 200,
                                WindowStartupLocation = WindowStartupLocation.CenterScreen
                            };
                            dialog.Show();
                        }
                        catch (Exception dialogEx)
                        {
                            Log.Error(dialogEx, "[CustomersView] Erro ao tentar mostrar diálogo de erro");
                        }
                    });
                };
                
                // Inicializar os componentes da interface
                InitializeComponent();
                
                // Registrar manipulador de eventos para anexação à árvore visual
                this.AttachedToVisualTree += (s, e) => 
                {
                    // Garantir que isso execute na thread da UI
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Log.Debug("[CustomersView] Evento AttachedToVisualTree disparado");
                        CustomersView_AttachedToVisualTree();
                    }
                    else
                    {
                        Log.Warning("[CustomersView] AttachedToVisualTree chamado de thread incorreta, redirecionando para thread da UI");
                        Dispatcher.UIThread.Post(() => 
                        {
                            Log.Debug("[CustomersView] Evento AttachedToVisualTree redirecionado para thread da UI");
                            CustomersView_AttachedToVisualTree();
                        });
                    }
                };
                
                // Registrar manipulador para quando desanexar da árvore visual
                this.DetachedFromVisualTree += (s, e) => 
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Log.Debug("[CustomersView] Evento DetachedFromVisualTree disparado");
                        CustomersView_DetachedFromVisualTree();
                    }
                    else
                    {
                        Log.Warning("[CustomersView] DetachedFromVisualTree chamado de thread incorreta, redirecionando para thread da UI");
                        Dispatcher.UIThread.Post(() => 
                        {
                            Log.Debug("[CustomersView] Evento DetachedFromVisualTree redirecionado para thread da UI");
                            CustomersView_DetachedFromVisualTree();
                        });
                    }
                };
                
                Log.Debug("[CustomersView] View construída com sucesso");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[CustomersView] Erro crítico durante a construção da view: {Message}", ex.Message);
                
                // Tente exibir uma mensagem mesmo em caso de falha grave
                try
                {
                    // Usar Post em vez de chamar diretamente
                    Dispatcher.UIThread.Post(() => 
                    {
                        var dialog = new Avalonia.Controls.Window
                        {
                            Title = "Erro na Aplicação",
                            Content = new TextBlock { 
                                Text = "Ocorreu um erro grave durante a inicialização. Por favor, verifique os logs.",
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                Margin = new Thickness(20)
                            },
                            Width = 400,
                            Height = 200,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        dialog.Show();
                    });
                }
                catch (Exception dialogEx)
                {
                    Log.Error(dialogEx, "[CustomersView] Não foi possível mostrar o diálogo de erro");
                }
            }
        }
        
        // Método para capturar globalmente exceções não tratadas
        private void GlobalExceptionHandler(Exception exception)
        {
            try
            {
                Log.Fatal(exception, "[CustomersView] Exceção não tratada capturada pelo handler global: {Message}", exception?.Message);
                
                // Mostrar erro na UI
                Dispatcher.UIThread.Post(() => {
                    try
                    {
                        // Exibir mensagem recuperável em vez de quebrar a aplicação
                        var vm = DataContext as ViewModels.CustomersViewModel;
                        if (vm != null)
                        {
                            vm.ErrorMessage = $"Erro: {exception?.Message}. Tente novamente.";
                            vm.HasError = true;
                        }
                    }
                    catch (Exception uiEx)
                    {
                        Log.Error(uiEx, "[CustomersView] Erro ao exibir mensagem de erro na UI");
                    }
                });
            }
            catch (Exception handlerEx)
            {
                Log.Fatal(handlerEx, "[CustomersView] Erro no handler de exceções não tratadas");
            }
        }
        
        private void CustomersView_AttachedToVisualTree()
        {
            try
            {
                Log.Debug("[CustomersView] View attached to visual tree, configuring controls");
                
                // Configurar TextBox para Nome
                var nameTextBox = this.FindControl<TextBox>("NameTextBox");
                if (nameTextBox != null)
                {
                    nameTextBox.GotFocus -= TextBox_GotFocus;
                    nameTextBox.GotFocus += TextBox_GotFocus;
                    
                    nameTextBox.LostFocus -= TextBox_LostFocus;
                    nameTextBox.LostFocus += TextBox_LostFocus;
                    
                    nameTextBox.TextInput -= TextBox_TextInput;
                    nameTextBox.TextInput += TextBox_TextInput;
                    
                    Log.Debug("[CustomersView] Successfully attached events to NameTextBox");
                }
                
                // Configurar TextBox para Documento
                var documentTextBox = this.FindControl<TextBox>("DocumentTextBox");
                if (documentTextBox != null)
                {
                    documentTextBox.GotFocus -= TextBox_GotFocus;
                    documentTextBox.GotFocus += TextBox_GotFocus;
                    
                    documentTextBox.LostFocus -= TextBox_LostFocus;
                    documentTextBox.LostFocus += TextBox_LostFocus;
                    
                    documentTextBox.TextInput -= TextBox_TextInput;
                    documentTextBox.TextInput += TextBox_TextInput;
                    
                    Log.Debug("[CustomersView] Successfully attached events to DocumentTextBox");
                }
                
                // Configurar TextBox para Email
                var emailTextBox = this.FindControl<TextBox>("EmailTextBox");
                if (emailTextBox != null)
                {
                    emailTextBox.GotFocus -= TextBox_GotFocus;
                    emailTextBox.GotFocus += TextBox_GotFocus;
                    
                    emailTextBox.LostFocus -= TextBox_LostFocus;
                    emailTextBox.LostFocus += TextBox_LostFocus;
                    
                    emailTextBox.TextInput -= TextBox_TextInput;
                    emailTextBox.TextInput += TextBox_TextInput;
                    
                    Log.Debug("[CustomersView] Successfully attached events to EmailTextBox");
                }
                
                // Configurar TextBox para Telefone
                var phoneTextBox = this.FindControl<TextBox>("PhoneTextBox");
                if (phoneTextBox != null)
                {
                    phoneTextBox.GotFocus -= TextBox_GotFocus;
                    phoneTextBox.GotFocus += TextBox_GotFocus;
                    
                    phoneTextBox.LostFocus -= TextBox_LostFocus;
                    phoneTextBox.LostFocus += TextBox_LostFocus;
                    
                    phoneTextBox.TextInput -= TextBox_TextInput;
                    phoneTextBox.TextInput += TextBox_TextInput;
                    
                    Log.Debug("[CustomersView] Successfully attached events to PhoneTextBox");
                }
                
                // Configurar ComboBox para Tipo de Cliente
                var typeComboBox = this.FindControl<ComboBox>("CustomerTypeComboBox");
                if (typeComboBox != null)
                {
                    typeComboBox.GotFocus -= ComboBox_GotFocus;
                    typeComboBox.GotFocus += ComboBox_GotFocus;
                    
                    typeComboBox.LostFocus -= ComboBox_LostFocus;
                    typeComboBox.LostFocus += ComboBox_LostFocus;
                    
                    typeComboBox.SelectionChanged -= ComboBox_SelectionChanged;
                    typeComboBox.SelectionChanged += ComboBox_SelectionChanged;
                    
                    Log.Debug("[CustomersView] Successfully attached events to CustomerTypeComboBox");
                }
                
                // Configurar DataGrids
                var customersListGrid = this.FindControl<DataGrid>("CustomersList");
                if (customersListGrid != null)
                {
                    customersListGrid.SelectionChanged -= DataGrid_SelectionChanged;
                    customersListGrid.SelectionChanged += DataGrid_SelectionChanged;
                    
                    Log.Debug("[CustomersView] Successfully attached events to CustomersList DataGrid");
                }
                
                var searchResultsGrid = this.FindControl<DataGrid>("SearchResultsList");
                if (searchResultsGrid != null)
                {
                    searchResultsGrid.SelectionChanged -= DataGrid_SelectionChanged;
                    searchResultsGrid.SelectionChanged += DataGrid_SelectionChanged;
                    
                    Log.Debug("[CustomersView] Successfully attached events to SearchResultsList DataGrid");
                }
                
                // Configurar botões
                ConfigureButtonEvents("BtnSaveCustomer");
                ConfigureButtonEvents("BtnSearchCustomer");
                ConfigureButtonEvents("BtnEditCustomer");
                ConfigureButtonEvents("BtnDeleteCustomer");
                ConfigureButtonEvents("BtnListCustomers");
                ConfigureButtonEvents("BtnAddCustomer");
                ConfigureButtonEvents("BtnSearchCustomers");
                ConfigureButtonEvents("BtnRefresh");
                ConfigureButtonEvents("BtnEditSearchCustomer");
                ConfigureButtonEvents("BtnDeleteSearchCustomer");
                
                Log.Debug("[CustomersView] All controls configured successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in AttachedToVisualTree event");
            }
        }
        
        private void ConfigureButtonEvents(string buttonName)
        {
            try
            {
                var button = this.FindControl<Button>(buttonName);
                if (button != null)
                {
                    // Remover handler anterior para evitar duplicações
                    button.Click -= Button_Click;
                    button.Click += Button_Click;
                    
                    Log.Debug("[CustomersView] Successfully attached events to button: {ButtonName}", buttonName);
                }
                else
                {
                    Log.Warning("[CustomersView] Button not found: {ButtonName}", buttonName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error configuring button: {ButtonName}", buttonName);
            }
        }
        
        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            // Prevenção contra cliques rápidos e repetidos
            var button = sender as Button;
            if (button == null) return;
            
            // Desabilitar o botão imediatamente para evitar duplos cliques
            button.IsEnabled = false;
            
            // Garantir que isso execute sempre na thread da UI
            if (Dispatcher.UIThread.CheckAccess())
            {
                // Criar um guard de timeout para garantir que o botão será reabilitado
                var timer = new System.Timers.Timer(2000);
                timer.Elapsed += (s, args) => {
                    Dispatcher.UIThread.Post(() => {
                        try {
                            if (button != null && !button.IsEnabled)
                            {
                                button.IsEnabled = true;
                                Log.Debug("[CustomersView] Button re-enabled by safety timer: {ButtonName}", button.Name);
                            }
                        } catch (Exception ex) {
                            Log.Error(ex, "[CustomersView] Error in timer callback for button");
                        }
                        
                        timer.Dispose();
                    });
                };
                timer.AutoReset = false;
                timer.Start();
                
                // Processar o clique com segurança
                try
                {
                    SafeInvokeButtonCommand(button, e);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[CustomersView] Erro ao processar clique do botão: {ButtonName}", button.Name);
                    Dispatcher.UIThread.Post(() => button.IsEnabled = true);
                }
            }
            else
            {
                // Se não estamos na thread de UI, postar para a thread correta
                Dispatcher.UIThread.Post(() => {
                    try {
                        SafeInvokeButtonCommand(button, e);
                    }
                    catch (Exception ex) {
                        Log.Error(ex, "[CustomersView] Error processing button click from non-UI thread: {ButtonName}", button.Name);
                        button.IsEnabled = true;
                    }
                });
            }
            
            // Marcar o evento como tratado
            e.Handled = true;
        }
        
        private void SafeInvokeButtonCommand(Button button, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("[CustomersView] Processing click for button: {ButtonName}", button.Name);
                
                // Verificar se o botão tem um comando associado
                var command = button.Command;
                var parameter = button.CommandParameter;
                
                if (command != null && command.CanExecute(parameter))
                {
                    // Executar o comando
                    Log.Debug("[CustomersView] Executing command for button: {ButtonName}", button.Name);
                    command.Execute(parameter);
                    
                    // Reabilitar o botão após um curto delay
                    Task.Delay(500).ContinueWith(_ => 
                    {
                        Dispatcher.UIThread.Post(() => 
                        {
                            try 
                            {
                                button.IsEnabled = true;
                                Log.Debug("[CustomersView] Button re-enabled after command: {ButtonName}", button.Name);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "[CustomersView] Error re-enabling button in UI thread");
                            }
                        });
                    });
                }
                else
                {
                    Log.Warning("[CustomersView] Button has no command or command cannot execute: {ButtonName}", button.Name);
                    Dispatcher.UIThread.Post(() => button.IsEnabled = true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in SafeInvokeButtonCommand for button: {ButtonName}", button.Name);
                Dispatcher.UIThread.Post(() => button.IsEnabled = true);
                throw; // Propagar para o handler de nível superior
            }
        }
        
        private void TextBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            TextBox_GotFocus(sender as TextBox);
        }
        
        private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            TextBox_LostFocus(sender as TextBox);
        }
        
        private void TextBox_TextInput(object? sender, TextInputEventArgs e)
        {
            TextBox_TextInput(sender as TextBox, e.Text);
        }
        
        private void TextBox_GotFocus(TextBox? textBox)
        {
            try
            {
                if (textBox != null)
                {
                    Log.Debug("[CustomersView] TextBox got focus: {Name}, Text: {Text}", 
                        textBox.Name, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in TextBox_GotFocus event");
            }
        }
        
        private void TextBox_LostFocus(TextBox? textBox)
        {
            try
            {
                if (textBox != null)
                {
                    Log.Debug("[CustomersView] TextBox lost focus: {Name}, Text: {Text}", 
                        textBox.Name, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in TextBox_LostFocus event");
            }
        }
        
        private void TextBox_TextInput(TextBox? textBox, string? text)
        {
            if (_isProcessingTextChange || textBox == null) return;
            
            try
            {
                _isProcessingTextChange = true;
                
                Log.Debug("[CustomersView] TextBox text input: {Name}, Text: {Text}, Input: {Input}", 
                    textBox.Name, textBox.Text, text);
                
                // Processar o texto se necessário
                
                _isProcessingTextChange = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in TextBox_TextInput event");
                _isProcessingTextChange = false;
            }
        }
        
        // Métodos para manipulação de eventos do ComboBox
        private void ComboBox_GotFocus(object? sender, GotFocusEventArgs e)
        {
            ComboBox_GotFocus(sender as ComboBox);
        }
        
        private void ComboBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            ComboBox_LostFocus(sender as ComboBox);
        }
        
        private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ComboBox_SelectionChanged(sender as ComboBox);
        }
        
        private void ComboBox_GotFocus(ComboBox? comboBox)
        {
            if (comboBox == null) return;
            
            try
            {
                Log.Debug("[CustomersView] ComboBox got focus: {ControlName}", comboBox.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in ComboBox_GotFocus event");
            }
        }
        
        private void ComboBox_LostFocus(ComboBox? comboBox)
        {
            if (comboBox == null) return;
            
            try
            {
                Log.Debug("[CustomersView] ComboBox lost focus: {ControlName}", comboBox.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in ComboBox_LostFocus event");
            }
        }
        
        private void ComboBox_SelectionChanged(ComboBox? comboBox)
        {
            if (comboBox == null) return;
            
            try
            {
                Log.Debug("[CustomersView] ComboBox selection changed: {ControlName}, Selected index: {SelectedIndex}", 
                    comboBox.Name, comboBox.SelectedIndex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in ComboBox_SelectionChanged event");
            }
        }
        
        private void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;
            
            try
            {
                var selectedItem = dataGrid.SelectedItem;
                string gridName = dataGrid.Name ?? "UnknownGrid";
                
                Log.Debug("[CustomersView] DataGrid selection changed: {GridName}, Selected item: {HasSelection}", 
                    gridName, selectedItem != null ? "Yes" : "No");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Error in DataGrid_SelectionChanged event");
            }
        }

        private void CustomersView_DetachedFromVisualTree()
        {
            try
            {
                Log.Debug("[CustomersView] Desvinculando eventos dos controles para limpeza de memória");
                
                // Desvincular TextBoxes
                RemoveEventsFromControl<TextBox>("NameTextBox");
                RemoveEventsFromControl<TextBox>("DocumentTextBox");
                RemoveEventsFromControl<TextBox>("EmailTextBox");
                RemoveEventsFromControl<TextBox>("PhoneTextBox");
                
                // Desvincular ComboBox
                RemoveEventsFromControl<ComboBox>("CustomerTypeComboBox");
                
                // Desvincular DataGrids
                RemoveEventsFromControl<DataGrid>("CustomersList");
                RemoveEventsFromControl<DataGrid>("SearchResultsList");
                
                // Desvincular botões
                RemoveEventsFromControl<Button>("BtnSaveCustomer");
                RemoveEventsFromControl<Button>("BtnSearchCustomer");
                RemoveEventsFromControl<Button>("BtnEditCustomer");
                RemoveEventsFromControl<Button>("BtnDeleteCustomer");
                RemoveEventsFromControl<Button>("BtnRefresh");
                
                Log.Debug("[CustomersView] Limpeza de eventos concluída com sucesso");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Erro ao desvincular eventos dos controles");
            }
        }

        private void RemoveEventsFromControl<T>(string controlName) where T : Control
        {
            try 
            {
                var control = this.FindControl<T>(controlName);
                if (control != null)
                {
                    // Remove eventos específicos para cada tipo de controle
                    if (control is TextBox textBox)
                    {
                        textBox.GotFocus -= TextBox_GotFocus;
                        textBox.LostFocus -= TextBox_LostFocus;
                        textBox.TextInput -= TextBox_TextInput;
                        Log.Debug("[CustomersView] Eventos removidos de {ControlType}: {ControlName}", typeof(T).Name, controlName);
                    }
                    else if (control is ComboBox comboBox)
                    {
                        comboBox.GotFocus -= ComboBox_GotFocus;
                        comboBox.LostFocus -= ComboBox_LostFocus;
                        comboBox.SelectionChanged -= ComboBox_SelectionChanged;
                        Log.Debug("[CustomersView] Eventos removidos de {ControlType}: {ControlName}", typeof(T).Name, controlName);
                    }
                    else if (control is DataGrid dataGrid)
                    {
                        dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                        Log.Debug("[CustomersView] Eventos removidos de {ControlType}: {ControlName}", typeof(T).Name, controlName);
                    }
                    else if (control is Button button)
                    {
                        button.Click -= Button_Click;
                        Log.Debug("[CustomersView] Eventos removidos de {ControlType}: {ControlName}", typeof(T).Name, controlName);
                    }
                }
                else
                {
                    Log.Warning("[CustomersView] Controle não encontrado para remover eventos: {ControlName}", controlName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[CustomersView] Erro ao remover eventos do controle {ControlName}", controlName);
            }
        }
    }
} 