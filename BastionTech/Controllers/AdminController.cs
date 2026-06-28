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
            var ventas = await _supabaseService.GetVentasTotalesAsync();
            var tickets = await _supabaseService.GetTicketsAsync(); // <-- Agregamos esto

            ViewBag.TotalIngresos = ventas.Sum(v => v.Total);
            ViewBag.TotalVentas = ventas.Count;
            // Contamos solo los que requieren atención
            ViewBag.TicketsAbiertos = tickets.Count(t => t.EstadoTicket == "Pendiente" || t.EstadoTicket == "En Proceso");

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
        // ==========================================
        // 🛠️ 5. CONSOLA DE SOPORTE TÉCNICO
        // ==========================================
        // Nota: NO ponemos [Authorize(Roles="Admin")] aquí. Heredará el acceso para Administradores y Técnicos.

        public async Task<IActionResult> Tickets()
        {
            var tickets = await _supabaseService.GetTicketsAsync();
            return View(tickets);
        }

        public async Task<IActionResult> GestionarTicket(int id)
        {
            var ticket = await _supabaseService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound("Ticket no encontrado.");
            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> GestionarTicket(Models.TicketServicio modelo)
        {
            // Actualizamos la información en Supabase
            await _supabaseService.ActualizarTicketAsync(modelo);
            TempData["MensajeExito"] = "El estado del ticket y las notas técnicas se han actualizado correctamente.";
            return RedirectToAction("Tickets");
        }
    }
}