namespace AppTiendaVirtual.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace AppTiendaVirtual.Models
    {
        public class Categoria
        {
            public int Id { get; set; }

            [Required, MaxLength(100)]
            public string Nombre { get; set; } = string.Empty;

            public string? Descripcion { get; set; }

            public bool Activo { get; set; } = true;

            // Navegacion
            public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        }
    }
}


