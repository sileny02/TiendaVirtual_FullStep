using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class ImagenProducto
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [Required, MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        public string? CloudinaryId { get; set; }

        public bool EsPrincipal { get; set; } = false;

        // Navegacion
        public Producto? Producto { get; set; }
    }

}
