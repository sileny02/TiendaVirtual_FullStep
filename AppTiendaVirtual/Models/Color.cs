using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class Color
    
       


        {
            public int Id { get; set; }

            [Required, MaxLength(50)]
            public string Nombre { get; set; } = string.Empty;

            public string? HexCodigo { get; set; }

            // Navegacion
            public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
        }
    }

