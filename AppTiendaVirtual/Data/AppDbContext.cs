using AppTiendaVirtual.Models;
using AppTiendaVirtual.Models.AppTiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace AppTiendaVirtual.Data
{
    using Microsoft.EntityFrameworkCore;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            
            : base(options) { }
        
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<ImagenProducto> ImagenesProducto { get; set; }
        public DbSet<Promocion> Promociones { get; set; }
        public DbSet<Talla> Tallas { get; set; }
        public DbSet<Color> Colores { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}

