using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("ventas")]
    public class Venta : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("usuarioid")]
        public string? UsuarioId { get; set; }

        [Column("fechatransaccion")]
        public DateTime FechaTransaccion { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Completada";

        [Column("correo")]
        public string? Correo { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("metodopago")]
        public string? MetodoPago { get; set; }
    }
}