using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AssistenciaTecnicaApp.Models
{
    public enum CustomerType
    {
        [Description("Pessoa Física")]
        Individual,
        [Description("Pessoa Jurídica")]
        Company
    }

    public static class CustomerTypeExtensions
    {
        public static Array GetValues() 
        {
            return Enum.GetValues(typeof(CustomerType));
        }
    }

    public class Customer : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _document = string.Empty;
        private CustomerType _type;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _zipCode = string.Empty;
        private bool _active = true;
        private DateTime _createdAt = DateTime.Now;
        private DateTime? _updatedAt;
        private bool _isEditing;

        public int Id 
        { 
            get => _id; 
            set => SetProperty(ref _id, value); 
        }
        
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value); 
        }
        
        public string Document 
        { 
            get => _document; 
            set => SetProperty(ref _document, value); 
        }
        
        public CustomerType Type 
        { 
            get => _type; 
            set => SetProperty(ref _type, value); 
        }
        
        public string Phone 
        { 
            get => _phone; 
            set => SetProperty(ref _phone, value); 
        }
        
        public string Email 
        { 
            get => _email; 
            set => SetProperty(ref _email, value); 
        }
        
        public string Address 
        { 
            get => _address; 
            set => SetProperty(ref _address, value); 
        }
        
        public string City 
        { 
            get => _city; 
            set => SetProperty(ref _city, value); 
        }
        
        public string State 
        { 
            get => _state; 
            set => SetProperty(ref _state, value); 
        }
        
        public string ZipCode 
        { 
            get => _zipCode; 
            set => SetProperty(ref _zipCode, value); 
        }
        
        public bool Active 
        { 
            get => _active; 
            set => SetProperty(ref _active, value); 
        }
        
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set => SetProperty(ref _createdAt, value); 
        }
        
        public DateTime? UpdatedAt 
        { 
            get => _updatedAt; 
            set => SetProperty(ref _updatedAt, value); 
        }
        
        public bool IsEditing 
        { 
            get => _isEditing; 
            set => SetProperty(ref _isEditing, value); 
        }
        
        // Navigation property for service orders (to be implemented later)
        // public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 