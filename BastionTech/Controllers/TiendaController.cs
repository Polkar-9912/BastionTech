using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;
using BastionTech.Models;

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
        public async Task<IActionResult> Index()
        {
            var catalogo = await _supabaseService.GetProductosAsync();

            // Opcional: Podrías filtrar aquí para que solo muestre productos con Stock > 0
            // var catalogoActivo = catalogo.Where(p => p.Stock > 0 || p.EsServicio).ToList();

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

                // 🌟 PASO 1: Limpiamos el ID. Si viene "" o null, lo transformamos en un null real de C#
                string? clienteUuid = string.IsNullOrEmpty(pedido.ClienteId) ? null : pedido.ClienteId;

                // 1. Crear la Cabecera de la Venta
                var nuevaVenta = new Venta
                {
                    UsuarioId = clienteUuid, // <-- PASO 2: Usamos la variable limpia aquí
                    FechaTransaccion = DateTime.UtcNow,
                    Total = 0,
                    Estado = "Completada"
                };

                var ventaRegistrada = await _supabaseService.RegistrarVentaAsync(nuevaVenta);

                // 2. Procesar cada Item del Carrito
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
                        // ES SERVICIO IT: Generamos el Ticket de Soporte
                        var ticket = new TicketServicio
                        {
                            VentaDetalleId = detalleRegistrado.Id,
                            ClienteId = clienteUuid, // <-- PASO 3: Nada de "ANONIMO", pasamos el UUID limpio o null
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
                } // <-- Fin del foreach de los items

                // 5. Actualizamos la venta con el total real calculado
                ventaRegistrada.Total = totalReal;

                // ¡AQUÍ ESTÁ LA SOLUCIÓN! Enviamos el UPDATE con el total definitivo a la base de datos
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

        // DTO para estructurar los datos que recibiremos desde JavaScript
        public class CarritoGuardadoDTO
        {
            public int ProductoId { get; set; }
            public int Cantidad { get; set; }
            public bool EsServicio { get; set; }
        }
    }

    // ==========================================
    // 📦 DTOs (Data Transfer Objects) Auxiliares
    // ==========================================
    // Estas clases pequeñas sirven solo para recibir la información de Javascript
    public class PedidoCheckoutDTO
    {
        public string ClienteId { get; set; } = string.Empty; // Vendrá de la sesión de Supabase Auth
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