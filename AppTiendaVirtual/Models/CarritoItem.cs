namespace AppTiendaVirtual.Models
{
    public class CarritoItem
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public int TallaId { get; set; }
        public string NombreTalla { get; set; } = string.Empty;
        public int ColorId { get; set; }
        public string NombreColor { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
}
