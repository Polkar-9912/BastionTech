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
                // REGLA DE SEGURIDAD 1: Nunca confíes en el total que viene de Javascript. 
                // Lo calcularemos nosotros leyendo la base de datos.
                decimal totalReal = 0;

                // 1. Crear la Cabecera de la Venta (Aún sin Total, lo actualizamos al final)
                var nuevaVenta = new Venta
                {
                    UsuarioId = string.IsNullOrEmpty(pedido.ClienteId) ? null : pedido.ClienteId,
                    FechaTransaccion = DateTime.UtcNow,
                    Total = 0, // Lo sumaremos en el bucle
                    Estado = "Completada"
                };

                var ventaRegistrada = await _supabaseService.RegistrarVentaAsync(nuevaVenta);

                // 2. Procesar cada Item del Carrito
                foreach (var item in pedido.Items)
                {
                    // Obtenemos el producto real desde Supabase para evitar trampas en precios o stock
                    var productoReal = await _supabaseService.GetProductoByIdAsync(item.ProductoId);

                    if (productoReal == null) continue;

                    // REGLA DE SEGURIDAD 2: Validar Stock (Solo si es Hardware)
                    if (!productoReal.EsServicio && productoReal.Stock < item.Cantidad)
                    {
                        // Si falla, en un sistema real haríamos un Rollback, 
                        // aquí lanzamos error para que Javascript lo avise.
                        throw new Exception($"Stock insuficiente para: {productoReal.Nombre}. Solo quedan {productoReal.Stock}.");
                    }

                    // 3. Crear el detalle de la venta con el Precio Real de la BD
                    var detalle = new VentaDetalle
                    {
                        VentaId = ventaRegistrada.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = productoReal.Precio
                    };
                    var detalleRegistrado = await _supabaseService.RegistrarDetalleVentaAsync(detalle);

                    // Sumamos al total general
                    totalReal += (detalle.Cantidad * detalle.PrecioUnitario);

                    // 4. LA BIFURCACIÓN (Hardware vs Servicio)
                    if (productoReal.EsServicio)
                    {
                        // ES SERVICIO IT: Generamos el Ticket de Soporte
                        var ticket = new TicketServicio
                        {
                            VentaDetalleId = detalleRegistrado.Id,
                            ClienteId = pedido.ClienteId ?? "ANONIMO", // Dependerá del login de Supabase
                            EstadoTicket = "Pendiente",
                            FechaCreacion = DateTime.UtcNow,
                            NotasTecnicas = "Generado automáticamente tras la compra web."
                        };
                        await _supabaseService.RegistrarTicketServicioAsync(ticket);
                    }
                    else
                    {
                        // ES HARDWARE: Descontamos el stock
                        productoReal.Stock -= item.Cantidad;
                        await _supabaseService.ActualizarProductoAsync(productoReal);
                    }
                }

                // 5. Actualizamos la venta con el total real calculado
                ventaRegistrada.Total = totalReal;
                // Opcional: Aquí podrías hacer un update a la Venta para guardar el total real en Supabase.

                return Ok(new
                {
                    mensaje = "Compra procesada con éxito. Stock actualizado y tickets generados.",
                    ventaId = ventaRegistrada.Id
                });
            }
            catch (Exception ex)
            {
                // Si falta stock o algo explota, le respondemos a Javascript
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