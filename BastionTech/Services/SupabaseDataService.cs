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
            // 🌟 ESTRATEGIA DE INYECCIÓN DINÁMICA (Render + Local)
            _url = Environment.GetEnvironmentVariable("SUPABASE_URL")
                   ?? configuration["Supabase:Url"]
                   ?? throw new ArgumentNullException("Supabase URL no configurada");

            _key = Environment.GetEnvironmentVariable("SUPABASE_KEY")
                   ?? configuration["Supabase:Key"]
                   ?? configuration["Supabase:ApiKey"]
                   ?? throw new ArgumentNullException("Supabase ApiKey no configurada");
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

        public async Task InsertarProductoAsync(Models.Producto producto)
        {
            var client = await GetClientAsync();
            await client.From<Models.Producto>().Insert(producto);
        }

        public async Task ActualizarProductoAsync(Models.Producto producto)
        {
            var client = await GetClientAsync();
            await client.From<Models.Producto>().Update(producto);
        }

        public async Task EliminarProductoAsync(int id)
        {
            var client = await GetClientAsync();
            await client.From<Models.Producto>().Where(x => x.Id == id).Delete();
        }

        // ==========================================
        // 🛒 MOTOR DE VENTAS Y TICKETS (CHECKOUT)
        // ==========================================
        public async Task<Models.Venta> RegistrarVentaAsync(Models.Venta nuevaVenta)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Venta>().Insert(nuevaVenta);
            return response.Models.First();
        }

        public async Task<Models.VentaDetalle> RegistrarDetalleVentaAsync(Models.VentaDetalle detalle)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.VentaDetalle>().Insert(detalle);
            return response.Models.First();
        }

        public async Task RegistrarTicketServicioAsync(Models.TicketServicio ticket)
        {
            var client = await GetClientAsync();
            await client.From<Models.TicketServicio>().Insert(ticket);
        }

        public async Task<Models.Venta?> GetVentaByIdAsync(int id)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Venta>().Where(x => x.Id == id).Single();
            return response;
        }

        // ==========================================
        // 👥 GESTIÓN DE USUARIOS
        // ==========================================
        public async Task RegistrarUsuarioPerfilAsync(Models.Usuario usuario)
        {
            var client = await GetClientAsync();
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
            await client.From<Models.CarritoGuardado>().Where(x => x.UsuarioId == usuarioId).Delete();

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
            var response = await client.From<Models.CarritoGuardado>()
                .Select("*, productos(*)")
                .Where(x => x.UsuarioId == usuarioId)
                .Get();
            return response.Models;
        }

        // ==========================================
        // 📊 CONSULTAS FINANCIERAS (ADMIN PANEL)
        // ==========================================
        public async Task<List<Models.Venta>> GetVentasTotalesAsync()
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Venta>()
                                       .Select("*")
                                       .Order("fechatransaccion", Supabase.Postgrest.Constants.Ordering.Descending)
                                       .Get();
            return response.Models;
        }

        public async Task<List<Models.VentaDetalle>> GetDetallesDeVentaAsync(int ventaId)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.VentaDetalle>()
                                       .Select("*, productos(*)")
                                       .Where(x => x.VentaId == ventaId)
                                       .Get();
            return response.Models;
        }

        public async Task ActualizarVentaAsync(Models.Venta venta)
        {
            var client = await GetClientAsync();
            await client.From<Models.Venta>().Update(venta);
        }

        // ==========================================
        // 🛠️ GESTIÓN DE SOPORTE TÉCNICO (TICKETS)
        // ==========================================
        public async Task<List<Models.TicketServicio>> GetTicketsAsync()
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.TicketServicio>()
                                       .Select("*")
                                       .Order("fechacreacion", Supabase.Postgrest.Constants.Ordering.Descending)
                                       .Get();
            return response.Models;
        }

        public async Task<Models.TicketServicio?> GetTicketByIdAsync(int id)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.TicketServicio>().Where(x => x.Id == id).Single();
            return response;
        }

        public async Task ActualizarTicketAsync(Models.TicketServicio ticket)
        {
            var client = await GetClientAsync();
            await client.From<Models.TicketServicio>().Update(ticket);
        }

        public async Task<List<Models.Usuario>> GetTecnicosDisponiblesAsync()
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.Usuario>()
                                       .Where(x => x.Rol == "Tecnico")
                                       .Get();
            return response.Models;
        }

        public async Task<List<Models.TicketServicio>> GetTicketsPorTecnicoAsync(string tecnicoId)
        {
            var client = await GetClientAsync();
            var response = await client.From<Models.TicketServicio>()
                                       .Where(x => x.TecnicoAsignadoId == tecnicoId)
                                       .Order("fechacreacion", Supabase.Postgrest.Constants.Ordering.Descending)
                                       .Get();
            return response.Models;
        }
    }
}