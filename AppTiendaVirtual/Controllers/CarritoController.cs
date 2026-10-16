using AppTiendaVirtual.Data;
using AppTiendaVirtual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTiendaVirtual.Controllers
{
    public class CarritoController : Controller
    {
        private readonly AppDbContext _db;

        public CarritoController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Carrito
        public IActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        // POST /Carrito/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, int tallaId, int colorId, int cantidad = 1)
        {
            // Verificar stock
            var inventario = await _db.Inventarios
                .Include(i => i.Talla)
                .Include(i => i.Color)
                .FirstOrDefaultAsync(i =>
                    i.ProductoId == productoId &&
                    i.TallaId == tallaId &&
                    i.ColorId == colorId);

            if (inventario == null || inventario.Stock < cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible.";
                return RedirectToAction("Detalle", "Producto", new { id = productoId });
            }

            var producto = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Promociones)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null) return NotFound();

            // Obtener carrito de sesion
            var carrito = ObtenerCarrito();

            // Verificar si ya existe ese item
            var itemExistente = carrito.FirstOrDefault(i =>
                i.ProductoId == productoId &&
                i.TallaId == tallaId &&
                i.ColorId == colorId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                var precioFinal = producto.Promociones
                    .FirstOrDefault(p => p.Activa)?.PrecioOferta ?? producto.Precio;

                carrito.Add(new CarritoItem
                {
                    ProductoId = productoId,
                    NombreProducto = producto.Nombre,
                    ImagenUrl = producto.Imagenes.FirstOrDefault()?.Url ?? "",
                    TallaId = tallaId,
                    NombreTalla = inventario.Talla?.Numero ?? "",
                    ColorId = colorId,
                    NombreColor = inventario.Color?.Nombre ?? "",
                    Precio = precioFinal,
                    Cantidad = cantidad
                });
            }

            GuardarCarrito(carrito);
            TempData["Exito"] = "Producto agregado al carrito.";
            return RedirectToAction("Index");
        }

        // POST /Carrito/Eliminar
        [HttpPost]
        public IActionResult Eliminar(int productoId, int tallaId, int colorId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i =>
                i.ProductoId == productoId &&
                i.TallaId == tallaId &&
                i.ColorId == colorId);

            if (item != null) carrito.Remove(item);

            GuardarCarrito(carrito);
            return RedirectToAction("Index");
        }

        // GET /Carrito/Vaciar
        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove("Carrito");
            return RedirectToAction("Index");
        }

        // ---- Helpers de sesion ----
        private List<CarritoItem> ObtenerCarrito()
        {
            var json = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(json)) return new List<CarritoItem>();
            return System.Text.Json.JsonSerializer.Deserialize<List<CarritoItem>>(json)
                   ?? new List<CarritoItem>();
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", json);
        }
    }
}
