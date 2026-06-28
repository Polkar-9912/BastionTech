using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("ventas_detalle")]
    public class VentaDetalle : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("ventaid")]
        public int VentaId { get; set; }

        [Column("productoid")]
        public int ProductoId { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("preciounitario")]
        public decimal PrecioUnitario { get; set; }

        // ==========================================
        // 🌉 PUENTE RELACIONAL (JOIN)
        // ==========================================

        // 1. Definimos la relación Left Join usando la columna local "productoid"
        [Reference(typeof(Models.Producto), joinType: ReferenceAttribute.JoinType.Left, foreignKey: "productoid")]

        // 2. Le decimos al serializador que busque los datos dentro del bloque JSON "productos"
        
        public Models.Producto Producto { get; set; }

        // 3. Inicializamos el objeto en el constructor para evitar excepciones de referencia nula
        public VentaDetalle()
        {
            Producto = new Models.Producto();
        }
    }
}