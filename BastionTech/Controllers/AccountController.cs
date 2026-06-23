using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;

namespace BastionTech.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseDataService _supabaseService;

        public AccountController(SupabaseDataService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ==========================================
        // 🔑 1. INICIAR SESIÓN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Lógica futura: 
            // 1. _supabaseService.Auth.SignIn(email, password)
            // 2. Crear las Cookies de seguridad en .NET
            // 3. Redirigir según el Rol (Cliente -> Tienda, Admin -> Dashboard)
            return RedirectToAction("Index", "Tienda");
        }

        // ==========================================
        // 📝 2. REGISTRO DE NUEVOS CLIENTES
        // ==========================================
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(string nombre, string email, string password)
        {
            // Lógica futura: Registrar al usuario en Supabase Auth y en la tabla "Usuarios"
            return RedirectToAction("Login");
        }

        // ==========================================
        // 🚪 3. CERRAR SESIÓN
        // ==========================================
        public IActionResult Logout()
        {
            // Destruir cookies y cerrar sesión en Supabase
            return RedirectToAction("Index", "Tienda");
        }
    }
}