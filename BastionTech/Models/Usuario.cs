using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BastionTech.Models
{
    [Table("usuarios")]
    public class Usuario : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = null!;

        [Column("nombrecompleto")]
        public string NombreCompleto { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("rol")]
        public string Rol { get; set; } = null!;

        [Column("fecharegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}