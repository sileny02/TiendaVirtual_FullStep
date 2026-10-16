using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Rol { get; set; } = "Admin";
    }
}
