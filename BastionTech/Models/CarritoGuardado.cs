using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization; // 1. IMPORTANTE: Agrega esto

namespace BastionTech.Models
{
    [Table("carritos_guardados")]
    public class CarritoGuardado : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonIgnore] // 2. Ignoramos los metadatos de Supabase
        public int Id { get; set; }

        [Column("usuario_id")]
        [JsonIgnore] // No necesitamos enviar el ID de usuario al JS
        public string UsuarioId { get; set; } = string.Empty;

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("es_servicio")]
        public bool EsServicio { get; set; }

        [Column("fecha_actualizacion")]
        [JsonIgnore] // Tampoco necesitamos enviar la fecha al JS
        public DateTime FechaActualizacion { get; set; }

        [Reference(typeof(Models.Producto), joinType: ReferenceAttribute.JoinType.Left, foreignKey: "producto_id")]
        [Column("productos")] // <-- ESTE ES EL PUENTE QUE FALTABA
        public Models.Producto Producto { get; set; }

        // Constructor para inicializar el objeto Producto
        public CarritoGuardado()
        {
            Producto = new Models.Producto();
        }
    }
}