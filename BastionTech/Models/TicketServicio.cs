using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("tickets_servicio")]
    public class TicketServicio : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // El 'int?' con el signo de interrogación indica a C# que ahora acepta valores nulos
        [Column("ventadetalleid")]
        public int? VentaDetalleId { get; set; }

        // El cliente que reporta el fallo (null si es un invitado sin cuenta)
        [Column("clienteid")]
        public string? ClienteId { get; set; }

        // NUEVO: UUID del empleado de soporte técnico que tiene el caso
        [Column("tecnicoasignadoid")]
        public string? TecnicoAsignadoId { get; set; }

        // NUEVO: Título descriptivo del problema para los tickets manuales
        [Column("asunto")]
        public string? Asunto { get; set; }

        [Column("estadoticket")]
        public string EstadoTicket { get; set; } = "Pendiente";

        [Column("fechacreacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("notastecnicas")]
        public string? NotasTecnicas { get; set; }

        [Column("telefono")]
        public string Telefono { get; set; }

        // ==========================================
        // 🌉 PUENTES RELACIONALES (JOINS)
        // ==========================================

        // Puente para saber quién es el técnico asignado
        [Reference(typeof(Models.Usuario), joinType: ReferenceAttribute.JoinType.Left, foreignKey: "tecnicoasignadoid")]
        public Models.Usuario Tecnico { get; set; }

        public TicketServicio()
        {
            Tecnico = new Models.Usuario();
        }
    }
}