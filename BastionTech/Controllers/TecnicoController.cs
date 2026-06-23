using Microsoft.AspNetCore.Mvc;
using BastionTech.Services;

namespace BastionTech.Controllers
{
    // [Authorize(Roles = "Tecnico")] 
    public class TecnicoController : Controller
    {
        private readonly SupabaseDataService _supabaseService;

        public TecnicoController(SupabaseDataService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ==========================================
        // 📋 1. BANDEJA DE TICKETS ASIGNADOS
        // ==========================================
        public IActionResult MisTickets()
        {
            // Lógica futura: Obtener el ID del técnico logueado desde la cookie
            // string tecnicoId = User.Claims.FirstOrDefault(c => c.Type == "Id").Value;
            // var tickets = await _supabaseService.GetTicketsPorTecnicoAsync(tecnicoId);

            return View(); // Pasaremos la lista de tickets a la vista
        }
    }
}