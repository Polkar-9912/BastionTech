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
    }
}