using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;

namespace BastionTech.Controllers
{
    // [Authorize(Roles = "Admin")] // Lo descomentaremos cuando configuremos las cookies de sesión
    public class AdminController : Controller
    {
        private readonly SupabaseDataService _supabaseService;

        public AdminController(SupabaseDataService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ==========================================
        // 📊 1. DASHBOARD PRINCIPAL
        // ==========================================
        public IActionResult Dashboard()
        {
            // Aquí cargaremos métricas: Total de ventas, tickets abiertos, etc.
            return View();
        }

        // ==========================================
        // 📦 2. GESTIÓN DE INVENTARIO (CRUD)
        // ==========================================
        public async Task<IActionResult> Inventario()
        {
            // Reutilizamos el servicio para traer los productos a la vista de tabla del admin
            var productos = await _supabaseService.GetProductosAsync();
            return View(productos);
        }

        // ==========================================
        // 📄 3. REPORTES
        // ==========================================
        public IActionResult Reportes()
        {
            // Vista para descargar los PDFs de los reportes del Blue Team
            return View();
        }
    }
}