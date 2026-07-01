using System.Collections.Generic;

namespace BastionTech.Models
{
    public class MisComprasViewModel
    {
        /// <summary>
        /// Colección completa de transacciones financieras realizadas por el cliente autenticado.
        /// </summary>
        public List<Venta> Compras { get; set; } = new List<Venta>();
    }
}