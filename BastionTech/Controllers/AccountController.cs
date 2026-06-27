using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;
using BastionTech.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BastionTech.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseDataService _supabaseService;

        public AccountController(SupabaseDataService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está logueado, lo mandamos a la tienda
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Tienda");
            return View();
        }

        // ==========================================
        // 🛡️ PUENTE: CREAR COOKIE DE .NET
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CrearSesionCookie([FromBody] UsuarioDTO dto)
        {
            try
            {
                // 1. Buscamos el usuario en nuestra tabla para saber su ROL
                var usuarioDB = await _supabaseService.GetUsuarioByIdAsync(dto.Id);

                if (usuarioDB == null) return Unauthorized(new { mensaje = "Usuario no encontrado en la base de datos." });

                // 2. Creamos los Claims (La credencial de .NET)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioDB.Id),
                    new Claim(ClaimTypes.Name, usuarioDB.NombreCompleto),
                    new Claim(ClaimTypes.Email, usuarioDB.Email),
                    new Claim(ClaimTypes.Role, usuarioDB.Rol) // Aquí inyectamos el ROL
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Emitimos la Cookie cifrada al navegador
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Ok(new { mensaje = "Cookie generada exitosamente", rol = usuarioDB.Rol });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno", detalle = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SincronizarPerfil([FromBody] UsuarioDTO dto)
        {
            try
            {
                var nuevoUsuario = new Usuario
                {
                    Id = dto.Id,
                    NombreCompleto = dto.NombreCompleto,
                    Email = dto.Email,
                    Rol = "Cliente",
                    FechaRegistro = DateTime.UtcNow
                };

                await _supabaseService.RegistrarUsuarioPerfilAsync(nuevoUsuario);
                return Ok(new { mensaje = "Perfil sincronizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al sincronizar perfil", detalle = ex.Message });
            }
        }

        // ==========================================
        // 🚪 CERRAR SESIÓN
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            // Borra la cookie de .NET
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Tienda");
        }

        public class UsuarioDTO
        {
            public string Id { get; set; } = string.Empty;
            public string NombreCompleto { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}