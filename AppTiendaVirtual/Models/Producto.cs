using AppTiendaVirtual.Models.AppTiendaVirtual.Models;
using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class Producto
    {
        public int Id { get; set; }

            [Required, MaxLength(200)]
            public string Nombre { get; set; } = string.Empty;

            public string? Descripcion { get; set; }

            [Required]
            public decimal Precio { get; set; }

            public int CategoriaId { get; set; }
            public bool Activo { get; set; } = true;
            public DateTime FechaCreacion { get; set; } = DateTime.Now;

            // Navegacion
            public Categoria? Categoria { get; set; }
            public ICollection<ImagenProducto> Imagenes { get; set; } = new List<ImagenProducto>();
            public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
            public ICollection<Promocion> Promociones { get; set; } = new List<Promocion>();
        }
    }

