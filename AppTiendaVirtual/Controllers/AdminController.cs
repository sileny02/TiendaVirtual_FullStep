using Microsoft.AspNetCore.Authorization;
using AppTiendaVirtual.Data;
using AppTiendaVirtual.Models;
using AppTiendaVirtual.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace AppTiendaVirtual.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
   
    public class AdminController : Controller
    {

        private readonly AppDbContext _db;
        private readonly CloudinaryService _cloud;

        public AdminController(AppDbContext db, CloudinaryService cloud)
        {
            _db = db;
            _cloud = cloud;
        }

        // GET /admin
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var productos = await _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .ToListAsync();
            return View(productos);
        }

        // GET /admin/crear
        [HttpGet("crear")]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _db.Categorias.ToListAsync();
            ViewBag.Tallas = await _db.Tallas.ToListAsync();
            ViewBag.Colores = await _db.Colores.ToListAsync();
            return View();
        }

        // POST /admin/crear
        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
    Producto producto,
    List<IFormFile>? imagenes,
    List<int> tallaIds,
    List<int> colorIds,
    List<int> stocks)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.ToListAsync();
                ViewBag.Tallas = await _db.Tallas.ToListAsync();
                ViewBag.Colores = await _db.Colores.ToListAsync();
                return View(producto);
            }

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();



            // Guardar inventario inicial
            for (int i = 0; i < tallaIds.Count; i++)
            {
                if (tallaIds[i] > 0 && colorIds[i] > 0)
                {
                    _db.Inventarios.Add(new Inventario
                    {
                        ProductoId = producto.Id,
                        TallaId = tallaIds[i],
                        ColorId = colorIds[i],
                        Stock = stocks[i]
                    });
                }
            }

            // Subir imagenes a Cloudinary
            if (imagenes != null)
            {
                bool primeraImagen = true;
                foreach (var img in imagenes.Where(i => i != null && i.Length > 0))
                {
                    try
                    {
                        var resultado = await _cloud.SubirImagenAsync(img);
                        if (resultado?.SecureUrl != null)
                        {
                            _db.ImagenesProducto.Add(new ImagenProducto
                            {
                                ProductoId = producto.Id,
                                Url = resultado.SecureUrl.ToString(),
                                CloudinaryId = resultado.PublicId ?? "",
                                EsPrincipal = primeraImagen
                            });
                            primeraImagen = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Error subiendo imagen: {ex.Message}";
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET /admin/editar/5
        [HttpGet("editar/{id}")]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            ViewBag.Categorias = await _db.Categorias.ToListAsync();
            ViewBag.Tallas = await _db.Tallas.ToListAsync();
            ViewBag.Colores = await _db.Colores.ToListAsync();
            return View(producto);
        }

        // POST /admin/editar/5
        [HttpPost("editar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto, List<IFormFile>? imagenes)
        {
            if (id != producto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.ToListAsync();
                return View(producto);
            }

            _db.Productos.Update(producto);

            if (imagenes != null)
            {
                foreach (var img in imagenes.Where(i => i != null && i.Length > 0))
                {
                    var resultado = await _cloud.SubirImagenAsync(img);
                    _db.ImagenesProducto.Add(new ImagenProducto
                    {
                        ProductoId = producto.Id,
                        Url = resultado.SecureUrl.ToString(),
                        CloudinaryId = resultado.PublicId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET /admin/toggle/5
        [HttpGet("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            producto.Activo = !producto.Activo;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET /admin/eliminarimagen/5
        [HttpGet("eliminarimagen/{id}")]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            var imagen = await _db.ImagenesProducto.FindAsync(id);
            if (imagen == null) return NotFound();

            if (!string.IsNullOrEmpty(imagen.CloudinaryId))
                await _cloud.EliminarImagenAsync(imagen.CloudinaryId);

            _db.ImagenesProducto.Remove(imagen);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET /admin/inventario/5
        [HttpGet("inventario/{id}")]
        public async Task<IActionResult> Inventario(int id)
        {
            var producto = await _db.Productos
                .Include(p => p.Inventarios).ThenInclude(i => i.Talla)
                .Include(p => p.Inventarios).ThenInclude(i => i.Color)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            ViewBag.Tallas = await _db.Tallas.ToListAsync();
            ViewBag.Colores = await _db.Colores.ToListAsync();
            return View(producto);
        }

        // POST /admin/agregarinventario
        [HttpPost("agregarinventario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarInventario(int productoId, int tallaId, int colorId, int stock)
        {
            var existente = await _db.Inventarios.FirstOrDefaultAsync(i =>
                i.ProductoId == productoId &&
                i.TallaId == tallaId &&
                i.ColorId == colorId);

            if (existente != null)
            {
                existente.Stock += stock;
            }
            else
            {
                _db.Inventarios.Add(new Inventario
                {
                    ProductoId = productoId,
                    TallaId = tallaId,
                    ColorId = colorId,
                    Stock = stock
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Inventario", new { id = productoId });
        }

        // POST /admin/eliminarinventario
        [HttpPost("eliminarinventario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarInventario(int id, int productoId)
        {
            var inv = await _db.Inventarios.FindAsync(id);
            if (inv != null)
            {
                _db.Inventarios.Remove(inv);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Inventario", new { id = productoId });
        }

        // GET /admin/promociones
        [HttpGet("promociones")]
        public async Task<IActionResult> Promociones()
        {
            var productos = await _db.Productos
                .Include(p => p.Promociones)
                .Include(p => p.Imagenes)
                .Where(p => p.Activo)
                .ToListAsync();
            return View(productos);
        }

        // POST /admin/promociones/agregar
        [HttpPost("promociones/agregar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarPromocion(
            int productoId, decimal precioOferta,
            DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Desactivar promociones anteriores del producto
            var anteriores = await _db.Promociones
                .Where(p => p.ProductoId == productoId && p.Activa)
                .ToListAsync();
            anteriores.ForEach(p => p.Activa = false);

            _db.Promociones.Add(new Promocion
            {
                ProductoId = productoId,
                PrecioOferta = precioOferta,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Activa = true
            });

            await _db.SaveChangesAsync();
            TempData["Exito"] = "Promoción agregada correctamente.";
            return RedirectToAction("Promociones");
        }

        // GET /admin/promociones/desactivar/5
        [HttpGet("promociones/desactivar/{id}")]
        public async Task<IActionResult> DesactivarPromocion(int id)
        {
            var promo = await _db.Promociones.FindAsync(id);
            if (promo != null)
            {
                promo.Activa = false;
                await _db.SaveChangesAsync();
            }
            TempData["Exito"] = "Promoción desactivada.";
            return RedirectToAction("Promociones");
        }

        // GET /admin/promociones/eliminar/5
        [HttpGet("promociones/eliminar/{id}")]
        public async Task<IActionResult> EliminarPromocion(int id)
        {
            var promo = await _db.Promociones.FindAsync(id);
            if (promo != null)
            {
                _db.Promociones.Remove(promo);
                await _db.SaveChangesAsync();
            }
            TempData["Exito"] = "Promoción eliminada.";
            return RedirectToAction("Promociones");
        }

        // GET /admin/reportes
        [HttpGet("reportes")]
        public async Task<IActionResult> Reportes()
        {
            // Productos con stock bajo o agotado
            var stockBajo = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .Where(p => p.Inventarios.Any())
                .OrderBy(p => p.Inventarios.Sum(i => i.Stock))
                .Take(10)
                .ToListAsync();

            // Productos con promocion activa
            var enOferta = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Promociones)
                .Where(p => p.Activo && p.Promociones.Any(pr => pr.Activa))
                .ToListAsync();

            // Productos agotados
            var agotados = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios)
                .Where(p => p.Activo)
                .Where(p => !p.Inventarios.Any() || p.Inventarios.Sum(i => i.Stock) == 0)
                .ToListAsync();

            // Resumen general
            ViewBag.TotalProductos = await _db.Productos.CountAsync(p => p.Activo);
            ViewBag.TotalCategorias = await _db.Categorias.CountAsync(c => c.Activo);
            ViewBag.TotalEnOferta = enOferta.Count;
            ViewBag.TotalAgotados = agotados.Count;
            ViewBag.StockTotal = await _db.Inventarios.SumAsync(i => i.Stock);

            ViewBag.StockBajo = stockBajo;
            ViewBag.EnOferta = enOferta;
            ViewBag.Agotados = agotados;

            return View();
        }
    }
}
