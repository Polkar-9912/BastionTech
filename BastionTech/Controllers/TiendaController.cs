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
            // Esta vista cargará vacía desde C#. 
            // Usaremos Javascript en el navegador para leer el LocalStorage 
            // y pintar los productos que el cliente haya ido agregando.
            return View();
        }

        // ==========================================
        // 💳 4. PROCESAR LA COMPRA (ENDPOINT API)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ProcesarCheckout([FromBody] PedidoCheckoutDTO pedido)
        {
            // Aquí llegará el carrito desde Javascript cuando el cliente le dé a "Pagar"
            // Por ahora, solo preparamos la estructura de respuesta.

            if (pedido == null || !pedido.Items.Any())
            {
                return BadRequest(new { mensaje = "El carrito está vacío." });
            }

            try
            {
                // [Lógica futura: Crear Venta, Detalles y Tickets usando _supabaseService]

                return Ok(new { mensaje = "Compra procesada con éxito", ventaId = 999 }); // ID simulado
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno procesando el pago.", error = ex.Message });
            }
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