using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class Talla
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        // Navegacion
        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    }
}

