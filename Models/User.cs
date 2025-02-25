using System;

namespace AssistenciaTecnicaApp.Models
{
    public enum UserRole
    {
        Admin,
        Gerente,
        Atendente,
        Tecnico
    }

    public class User
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public UserRole Cargo { get; set; }
        public int LojaId { get; set; }
    }
} 