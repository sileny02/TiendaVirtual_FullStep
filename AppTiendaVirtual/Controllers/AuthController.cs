using AppTiendaVirtual.Data;
using AppTiendaVirtual.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppTiendaVirtual.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Auth/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var hash = HashPassword(password);
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            return Redirect(returnUrl ?? "/admin");
        }

        // GET /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // GET /Auth/GenerarHash?password=tupassword
        public IActionResult GenerarHash(string password)
        {
            return Ok(HashPassword(password));
        }

        // GET /Auth/Registro
        public IActionResult Registro()
        {
            // Bloquear si ya existe un admin
            if (_db.Usuarios.Any())
                return RedirectToAction("Login");
            return View();
        }

        // POST /Auth/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(string email, string password)
        {
            if (_db.Usuarios.Any())
                return RedirectToAction("Login");

            _db.Usuarios.Add(new Usuario
            {
                Email = email,
                PasswordHash = HashPassword(password),
                Rol = "Admin"
            });
            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }
    }
}