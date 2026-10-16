using Microsoft.EntityFrameworkCore.Migrations;
using System.ComponentModel.DataAnnotations;

namespace AppTiendaVirtual.Models
{
    public class Promocion
    {
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [Required]
        public decimal PrecioOferta { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public bool Activa { get; set; } = true;

        // Navegacion
        public Producto? Producto { get; set; }
    }
}



