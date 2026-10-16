namespace AppTiendaVirtual.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int TallaId { get; set; }
        public int ColorId { get; set; }
        public int Stock { get; set; }

        public Producto? Producto { get; set; }
        public Talla? Talla { get; set; }
        public Color? Color { get; set; }
        public string EstadoStock => Stock switch
        {
            0 => "Agotado",
            <= 5 => "Bajo",
            _ => "Alto"
        };
    }
}

