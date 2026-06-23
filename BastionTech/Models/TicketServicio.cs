using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("tickets_servicio")]
    public class TicketServicio : BaseModel
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("ventadetalleid")]
        public int VentaDetalleId { get; set; }

        [Column("clienteid")]
        public string ClienteId { get; set; } = null!;

        [Column("tecnicoasignadoid")]
        public string? TecnicoAsignadoId { get; set; }

        [Column("estadoticket")]
        public string EstadoTicket { get; set; } = "Pendiente";

        [Column("notastecnicas")]
        public string? NotasTecnicas { get; set; }

        [Column("fechacreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}