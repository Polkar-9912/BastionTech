using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;

namespace BastionTech.Controllers
{
    // Candado perimetral: Solo personal autorizado entra al controlador
    [Authorize(Roles = "Admin,Tecnico")]
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
        public async Task<IActionResult> Dashboard()
        {
            // 1. Obtenemos toda la data financiera
            var ventas = await _supabaseService.GetVentasTotalesAsync();

            // 2. Calculamos las métricas y las mandamos por ViewBag
            ViewBag.TotalIngresos = ventas.Sum(v => v.Total);
            ViewBag.TotalVentas = ventas.Count;
            ViewBag.TicketsAbiertos = 0; // Lo conectaremos en la Fase 4

            // 3. Mandamos solo las 10 transacciones más recientes a la tabla
            var ultimasVentas = ventas.Take(10).ToList();

            return View(ultimasVentas);
        }

        // ==========================================
        // 🧾 4. DETALLE DE TRANSACCIÓN
        // ==========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DetalleVenta(int id)
        {
            var detalles = await _supabaseService.GetDetallesDeVentaAsync(id);

            if (detalles == null || !detalles.Any())
            {
                return NotFound("La transacción no existe o no tiene detalles registrados.");
            }

            ViewBag.VentaId = id;
            return View(detalles);
        }

        // ==========================================
        // 📦 2. GESTIÓN DE INVENTARIO (CRUD)
        // ==========================================
        // REGLA CRÍTICA: Bloqueo estricto a nivel de método. El técnico recibirá un 403 Forbidden aquí.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inventario()
        {
            var productos = await _supabaseService.GetProductosAsync();
            return View(productos);
        }
        // ==========================================
        // ➕ CREAR PRODUCTO (Create)
        // ==========================================
        [Authorize(Roles = "Admin")]
        public IActionResult CrearProducto()
        {
            return View(new Models.Producto()); // Mandamos un modelo vacío al formulario
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CrearProducto(Models.Producto producto)
        {
            if (ModelState.IsValid)
            {
                await _supabaseService.InsertarProductoAsync(producto);
                TempData["MensajeExito"] = "Artículo creado y añadido al catálogo correctamente.";
                return RedirectToAction("Inventario");
            }
            return View(producto);
        }

        // ==========================================
        // ✏️ EDITAR PRODUCTO (Update)
        // ==========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = await _supabaseService.GetProductoByIdAsync(id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarProducto(Models.Producto producto)
        {
            if (ModelState.IsValid)
            {
                await _supabaseService.ActualizarProductoAsync(producto);
                TempData["MensajeExito"] = "El artículo fue actualizado con éxito.";
                return RedirectToAction("Inventario");
            }
            return View(producto);
        }

        // ==========================================
        // 🗑️ ELIMINAR PRODUCTO (Delete)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            await _supabaseService.EliminarProductoAsync(id);
            TempData["MensajeExito"] = "El artículo ha sido eliminado definitivamente.";
            return RedirectToAction("Inventario");
        }

        // ==========================================
        // 📄 3. REPORTES
        // ==========================================
        [Authorize(Roles = "Admin")]
        public IActionResult Reportes()
        {
            return View();
        }
    }
}