using Supabase;

namespace BastionTech.Services
{
    public class SupabaseDataService
    {
        private readonly string _url;
        private readonly string _key;
        private Client? _client;

        public SupabaseDataService(IConfiguration configuration)
        {
            _url = configuration["Supabase:Url"] ?? throw new ArgumentNullException("Supabase URL no configurada");
            _key = configuration["Supabase:ApiKey"] ?? throw new ArgumentNullException("Supabase ApiKey no configurada");
        }

        public async Task<Client> GetClientAsync()
        {
            if (_client == null)
            {
                var options = new SupabaseOptions { AutoRefreshToken = true };
                _client = new Client(_url, _key, options);
                await _client.InitializeAsync();
            }
            return _client;
        }

        // ==========================================
        // 📦 CATÁLOGO E INVENTARIO
        // ==========================================

        public async Task<List<Models.Producto>> GetProductosAsync()
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Producto>().Get();
            return response.Models;
        }

        public async Task<Models.Producto?> GetProductoByIdAsync(int id)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Producto>().Where(x => x.Id == id).Single();
            return response;
        }

        // ==========================================
        // 🛒 MOTOR DE VENTAS Y TICKETS (CHECKOUT)
        // ==========================================

        public async Task<Models.Venta> RegistrarVentaAsync(Models.Venta nuevaVenta)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Venta>().Insert(nuevaVenta);
            return response.Models.First(); // Devuelve la venta creada con su nuevo ID
        }

        public async Task<Models.VentaDetalle> RegistrarDetalleVentaAsync(Models.VentaDetalle detalle)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.VentaDetalle>().Insert(detalle);
            return response.Models.First(); // Devuelve el detalle creado con su nuevo ID
        }

        public async Task RegistrarTicketServicioAsync(Models.TicketServicio ticket)
        {
            var client = await GetClientAsync();
            await client.From<Models.TicketServicio>().Insert(ticket);
        }
        public async Task ActualizarProductoAsync(Models.Producto producto)
        {
            var client = await GetClientAsync();
            // Supabase actualizará el registro completo basado en el ID del modelo
            await client.From<Models.Producto>().Update(producto);
        }
        // ==========================================
        // 👥 GESTIÓN DE USUARIOS
        // ==========================================
        public async Task RegistrarUsuarioPerfilAsync(Models.Usuario usuario)
        {
            var client = await GetClientAsync();
            // Usamos Upsert por si el usuario ya existe, no de error y solo actualice
            await client.From<Models.Usuario>().Upsert(usuario);
        }
        public async Task<Models.Usuario?> GetUsuarioByIdAsync(string id)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Usuario>().Where(x => x.Id == id).Single();
            return response;
        }
        // ==========================================
        // 🛒 GESTIÓN DEL CARRITO EN LA NUBE
        // ==========================================
        public async Task SincronizarCarritoAsync(string usuarioId, List<Models.CarritoGuardado> items)
        {
            var client = await GetClientAsync();

            // 1. Borramos el carrito anterior completo para evitar duplicados y conflictos
            await client.From<Models.CarritoGuardado>().Where(x => x.UsuarioId == usuarioId).Delete();

            // 2. Insertamos el nuevo carrito si es que hay productos
            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    item.UsuarioId = usuarioId;
                    item.FechaActualizacion = DateTime.UtcNow;
                }
                await client.From<Models.CarritoGuardado>().Insert(items);
            }
        }

        public async Task<List<Models.CarritoGuardado>> ObtenerCarritoGuardadoAsync(string usuarioId)
        {
            var client = await GetClientAsync();

            // Hacemos una consulta con JOIN para traer el producto asociado
            // Nota: Esto asume que tienes una relación definida en tu modelo 
            // o que podemos buscar los productos correspondientes.
            var response = await client.From<Models.CarritoGuardado>()
                .Select("*, productos(*)") // Traemos toda la info del producto relacionado
                .Where(x => x.UsuarioId == usuarioId)
                .Get();

            return response.Models;
        }
    }
}