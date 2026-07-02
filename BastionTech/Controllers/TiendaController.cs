using BastionTech.Models;
using BastionTech.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BastionTech.Controllers
{
    public class TiendaController : Controller
    {
        private readonly SupabaseDataService _supabaseService;

        // Inyectamos el servicio centralizado
        public TiendaController(SupabaseDataService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ==========================================
        // 🛍️ 1. CATÁLOGO PRINCIPAL
        // ==========================================
        public async Task<IActionResult> Index(string categoria = null)
        {
            // 1. Obtenemos el catálogo completo desde tu servicio de Supabase
            var catalogo = await _supabaseService.GetProductosAsync();

            // 2. Si el parámetro 'categoria' trae texto, filtramos la lista en memoria usando LINQ
            if (!string.IsNullOrEmpty(categoria))
            {
                catalogo = catalogo.Where(p => p.Categoria == categoria).ToList();

                // Almacenamos la categoría en el ViewBag para consumirla en la UI (Fase 3)
                ViewBag.CategoriaActual = categoria;
            }

            // 3. Enviamos la lista (filtrada o completa) a la vista
            return View(catalogo);
        }

        // ==========================================
        // 🔍 2. FICHA DE DETALLE DEL PRODUCTO
        // ==========================================
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _supabaseService.GetProductoByIdAsync(id);

            if (producto == null)
            {
                return NotFound("El producto que buscas no existe o fue retirado del catálogo.");
            }

            return View(producto);
        }

        // ==========================================
        // 🛒 3. VISTA DEL CARRITO DE COMPRAS
        // ==========================================
        public IActionResult Carrito()
        {
            // Bloquear acceso a staff interno
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Administrador") || User.IsInRole("Tecnico"))
                {
                    // Si es staff, lo mandamos de vuelta al catálogo
                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        // ==========================================
        // 💳 4. PROCESAR LA COMPRA (ENDPOINT API)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ProcesarCheckout([FromBody] PedidoCheckoutDTO pedido)
        {
            if (pedido == null || !pedido.Items.Any())
            {
                return BadRequest(new { mensaje = "El carrito está vacío." });
            }

            try
            {
                decimal totalReal = 0;
                string? clienteUuid = string.IsNullOrEmpty(pedido.ClienteId) ? null : pedido.ClienteId;

                // 🌟 ACTUALIZADO: Mapeamos las nuevas propiedades del DTO hacia la entidad de Supabase
                var nuevaVenta = new Venta
                {
                    UsuarioId = clienteUuid,
                    FechaTransaccion = DateTime.UtcNow,
                    Total = 0,
                    Estado = "Completada",
                    Correo = pedido.Correo,
                    Telefono = pedido.Telefono,
                    Direccion = pedido.Direccion,
                    MetodoPago = pedido.MetodoPago
                };

                var ventaRegistrada = await _supabaseService.RegistrarVentaAsync(nuevaVenta);

                // ... El resto del bucle foreach(var item in pedido.Items) se mantiene exactamente igual sin cambios

                foreach (var item in pedido.Items)
                {
                    var productoReal = await _supabaseService.GetProductoByIdAsync(item.ProductoId);
                    if (productoReal == null) continue;

                    if (!productoReal.EsServicio && productoReal.Stock < item.Cantidad)
                    {
                        throw new Exception($"Stock insuficiente para: {productoReal.Nombre}. Solo quedan {productoReal.Stock}.");
                    }

                    var detalle = new VentaDetalle
                    {
                        VentaId = ventaRegistrada.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = productoReal.Precio
                    };
                    var detalleRegistrado = await _supabaseService.RegistrarDetalleVentaAsync(detalle);

                    totalReal += (detalle.Cantidad * detalle.PrecioUnitario);

                    if (productoReal.EsServicio)
                    {
                        var ticket = new TicketServicio
                        {
                            VentaDetalleId = detalleRegistrado.Id,
                            ClienteId = clienteUuid,
                            EstadoTicket = "Pendiente",
                            FechaCreacion = DateTime.UtcNow,
                            NotasTecnicas = "Generado automáticamente tras la compra web."
                        };
                        await _supabaseService.RegistrarTicketServicioAsync(ticket);
                    }
                    else
                    {
                        productoReal.Stock -= item.Cantidad;
                        await _supabaseService.ActualizarProductoAsync(productoReal);
                    }
                }

                ventaRegistrada.Total = totalReal;
                await _supabaseService.ActualizarVentaAsync(ventaRegistrada);

                return Ok(new
                {
                    mensaje = "Compra procesada con éxito. Stock actualizado y tickets generados.",
                    ventaId = ventaRegistrada.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // ==========================================
        // 🛠️ 5. APERTURA MANUAL DE TICKETS (PÚBLICO)
        // ==========================================
        [HttpGet]
        public IActionResult AbrirTicket()
        {
            return View(new Models.TicketServicio());
        }

        [HttpPost]
        public async Task<IActionResult> AbrirTicket(Models.TicketServicio ticket)
        {
            try
            {
                ticket.EstadoTicket = "Pendiente";
                ticket.FechaCreacion = DateTime.UtcNow;

                // Ahora sí funciona porque este método está dentro del Controlador
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    ticket.ClienteId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                }

                await _supabaseService.RegistrarTicketServicioAsync(ticket);

                TempData["MensajeExito"] = "Tu solicitud de soporte ha sido registrada con éxito. Nuestro equipo técnico la revisará en breve.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hubo un error de conexión: " + ex.Message);
                return View(ticket);
            }
        }
        // ==========================================
        // 🧾 6. RECIBO Y CONFIRMACIÓN DE COMPRA
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Recibo(int id)
        {
            var venta = await _supabaseService.GetVentaByIdAsync(id);
            if (venta == null) return NotFound("El recibo solicitado no existe.");

            // Traemos los detalles de la compra (productos/servicios adquiridos)
            var detalles = await _supabaseService.GetDetallesDeVentaAsync(id);

            // Pasamos los detalles a través del ViewBag
            ViewBag.Detalles = detalles;

            return View(venta);
        }
        // ==========================================
        // 💳 HISTORIAL FINANCIERO DEL CLIENTE
        // ==========================================

        [HttpGet]
        [Authorize] // Restringe el acceso únicamente a usuarios que hayan iniciado sesión
        public async Task<IActionResult> MisCompras()
        {
            // 1. Extraer de forma segura el correo electrónico del usuario autenticado en la sesión
            var usuarioCorreo = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(usuarioCorreo))
            {
                // Mecanismo de defensa por si la sesión expira o es inválida
                return RedirectToAction("Index", "Tienda");
            }

            // 2. Consultar el repositorio global de ventas en Supabase
            var todasLasVentas = await _supabaseService.GetVentasTotalesAsync() ?? new List<Models.Venta>();

            // 3. MOTOR LINQ: Filtramos las ventas que pertenezcan al usuario y ordenamos de la más reciente a la más antigua
            var comprasFiltradas = todasLasVentas
                .Where(v => v.Correo != null && v.Correo.Equals(usuarioCorreo, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(v => v.FechaTransaccion)
                .ToList();

            // 4. Empaquetamos los datos procesados en el ViewModel
            var viewModel = new Models.MisComprasViewModel
            {
                Compras = comprasFiltradas
            };

            // 5. Enviamos el modelo a la vista (Views/Tienda/MisCompras.cshtml)
            return View(viewModel);
        }
    } // <-- AQUÍ SE CIERRA LA CLASE TIENDACONTROLLER

    // ==========================================
    // 📦 DTOs (Data Transfer Objects) Auxiliares
    // ==========================================
    public class CarritoGuardadoDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public bool EsServicio { get; set; }
    }

    public class PedidoCheckoutDTO
    {
        public string ClienteId { get; set; } = string.Empty;

        // NUEVOS CAMPOS AÑADIDOS
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;

        public List<ItemCarritoDTO> Items { get; set; } = new List<ItemCarritoDTO>();
        public decimal TotalCalculado { get; set; }
    }

    public class ItemCarritoDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool EsServicio { get; set; }
    }
}