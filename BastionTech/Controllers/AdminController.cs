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
        // 📊 PANEL DE REPORTES Y ANALÍTICA (GERENCIA)
        // ==========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reportes(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // 1. Inicialización de Fechas (Si llegan nulas, se asume "Hoy")
            DateTime inicio = fechaInicio ?? DateTime.Today;
            DateTime fin = fechaFin ?? DateTime.Today.AddDays(1).AddTicks(-1);

            var viewModel = new Models.ReportesViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin
            };

            // 2. Extraemos los conjuntos de datos globales desde Supabase
            var ventas = await _supabaseService.GetVentasTotalesAsync() ?? new List<Models.Venta>();
            var tickets = await _supabaseService.GetTicketsAsync() ?? new List<Models.TicketServicio>();

            // 3. Procesamiento Histórico (Fijo)
            viewModel.TotalOrdenes = ventas.Count;
            viewModel.IngresosTotales = ventas.Sum(v => v.Total);

            // 4. Filtro LINQ de Ventas (El motor del Periodo)
            viewModel.VentasPeriodo = ventas
                .Where(v => v.FechaTransaccion >= inicio && v.FechaTransaccion <= fin)
                .OrderByDescending(v => v.FechaTransaccion)
                .ToList();

            viewModel.IngresosPeriodo = viewModel.VentasPeriodo.Sum(v => v.Total);

            // 5. Métricas de Tickets (Globales por defecto, como solicitaste)
            viewModel.TicketPendientes = tickets.Count(t => t.EstadoTicket == "Pendiente");
            viewModel.TicketEnProceso = tickets.Count(t => t.EstadoTicket == "En Proceso");
            viewModel.TicketResueltos = tickets.Count(t => t.EstadoTicket == "Resuelto");

            // 6. Procesamiento del Ranking de Productos (Aplicado SOLO a las ventas del periodo)
            if (viewModel.VentasPeriodo.Any())
            {
                var todosLosDetalles = new List<Models.VentaDetalle>();

                foreach (var venta in viewModel.VentasPeriodo)
                {
                    var detalles = await _supabaseService.GetDetallesDeVentaAsync(venta.Id);
                    if (detalles != null)
                    {
                        todosLosDetalles.AddRange(detalles);
                    }
                }

                viewModel.ProductosMasVendidos = todosLosDetalles
                    .Where(d => d.Producto != null)
                    .GroupBy(d => d.ProductoId)
                    .Select(grupo => new Models.ProductoRanking
                    {
                        ProductoId = grupo.Key,
                        NombreProducto = grupo.First().Producto.Nombre,
                        CantidadVendida = grupo.Sum(d => d.Cantidad),
                        TotalRecaudado = grupo.Sum(d => d.Cantidad * d.PrecioUnitario),
                        EsServicio = grupo.First().Producto.EsServicio
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(5)
                    .ToList();
            }

            return View(viewModel);
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
        // 🛠️ 5. CONSOLA DE SOPORTE TÉCNICO
        // ==========================================

        // ==========================================
        // 🛠️ 5. BANDEJA DE TICKETS SEGMENTADA
        // ==========================================
        public async Task<IActionResult> Tickets()
        {
            List<Models.TicketServicio> tickets;

            if (User.IsInRole("Admin"))
            {
                // El Administrador mantiene el control total del centro de mando
                tickets = await _supabaseService.GetTicketsAsync();
            }
            else if (User.IsInRole("Tecnico"))
            {
                // Extraemos el UUID de la sesión actual del Técnico
                var tecnicoId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(tecnicoId))
                {
                    return Challenge(); // Redirige a Login por seguridad si no se encuentra la identidad
                }

                // Cargamos exclusivamente su carga de trabajo asignada
                tickets = await _supabaseService.GetTicketsPorTecnicoAsync(tecnicoId);
            }
            else
            {
                return Forbid(); // Bloqueo absoluto para cualquier otro rol no autorizado
            }

            return View(tickets);
        }

        public async Task<IActionResult> GestionarTicket(int id)
        {
            var ticket = await _supabaseService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound("Ticket no encontrado.");

            // 🌟 NUEVO: Si es Administrador, traemos el staff técnico para llenar el dropdown
            if (User.IsInRole("Admin"))
            {
                ViewBag.Tecnicos = await _supabaseService.GetTecnicosDisponiblesAsync();
            }

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> GestionarTicket(Models.TicketServicio modelo)
        {
            // Actualizamos el registro completo en Supabase (incluyendo el nuevo TecnicoAsignadoId)
            await _supabaseService.ActualizarTicketAsync(modelo);
            TempData["MensajeExito"] = "La orden de trabajo ha sido actualizada con éxito.";
            return RedirectToAction("Tickets");
        }
    }
}