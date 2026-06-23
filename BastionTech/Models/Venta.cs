using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("ventas")]
    public class Venta : BaseModel
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("usuarioid")]
        public string? UsuarioId { get; set; }

        [Column("fechatransaccion")]
        public DateTime FechaTransaccion { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Completada";
    }
}