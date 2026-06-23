using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("ventas_detalle")]
    public class VentaDetalle : BaseModel
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("ventaid")]
        public int VentaId { get; set; }

        [Column("productoid")]
        public int ProductoId { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("preciounitario")]
        public decimal PrecioUnitario { get; set; }
    }
}