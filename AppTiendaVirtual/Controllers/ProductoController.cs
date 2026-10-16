using AppTiendaVirtual.Data;
using AppTiendaVirtual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTiendaVirtual.Controllers
{
    public class ProductoController : Controller
    {
        private readonly AppDbContext _db;

        public ProductoController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Producto/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios).ThenInclude(i => i.Talla)
                .Include(p => p.Inventarios).ThenInclude(i => i.Color)
                .Include(p => p.Promociones)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // GET /Producto o /Producto/Index
        public async Task<IActionResult> Index(
    int? categoriaId, int? colorId, int? tallaId, string? buscar, int pagina = 1)
        {
            int productosPorPagina = 9;

            var query = _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Imagenes)
                .Include(p => p.Inventarios).ThenInclude(i => i.Talla)
                .Include(p => p.Inventarios).ThenInclude(i => i.Color)
                .Include(p => p.Promociones)
                .Where(p => p.Activo);

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(p =>
                    p.Nombre.Contains(buscar) ||
                    p.Descripcion!.Contains(buscar));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId);

            if (colorId.HasValue)
                query = query.Where(p =>
                    p.Inventarios.Any(i => i.ColorId == colorId && i.Stock > 0));

            if (tallaId.HasValue)
                query = query.Where(p =>
                    p.Inventarios.Any(i => i.TallaId == tallaId && i.Stock > 0));

            int totalProductos = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalProductos / (double)productosPorPagina);

            pagina = Math.Max(1, Math.Min(pagina, totalPaginas == 0 ? 1 : totalPaginas));

            var productos = await query
                .Skip((pagina - 1) * productosPorPagina)
                .Take(productosPorPagina)
                .ToListAsync();

            ViewBag.Categorias = await _db.Categorias.Where(c => c.Activo).ToListAsync();
            ViewBag.Colores = await _db.Colores.ToListAsync();
            ViewBag.Tallas = await _db.Tallas.ToListAsync();
            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.ColorSeleccionado = colorId;
            ViewBag.TallaSeleccionada = tallaId;
            ViewBag.Buscar = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProductos = totalProductos;

            return View(productos);
        }
    }
}
    