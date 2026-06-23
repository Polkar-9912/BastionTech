using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("productos")]
    public class Producto : BaseModel
    {
        [PrimaryKey("id", true)]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("descripcioncorta")]
        public string? DescripcionCorta { get; set; }

        [Column("especificacionestecnicas")]
        public string? EspecificacionesTecnicas { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [Column("esservicio")]
        public bool EsServicio { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("urlimagen")]
        public string? UrlImagen { get; set; }
    }
}