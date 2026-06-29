using System.Collections.Generic;

namespace BastionTech.Models
{
    public class ReportesViewModel
    {
        // ==========================================
        // 💰 METRICAS FINANCIERAS PRINCIPALES
        // ==========================================

        /// <summary>
        /// La sumatoria de los montos totales de todas las ventas registradas.
        /// </summary>
        public decimal IngresosTotales { get; set; }

        /// <summary>
        /// El conteo absoluto de transacciones comerciales procesadas.
        /// </summary>
        public int TotalOrdenes { get; set; }

        /// <summary>
        /// El ticket o gasto promedio por cada orden de compra (Bs. Ingresos / TotalOrdenes).
        /// </summary>
        public decimal TicketPromedio => TotalOrdenes > 0 ? IngresosTotales / TotalOrdenes : 0;


        // ==========================================
        // 🛡️ METRICAS OPERATIVAS (BLUE TEAM)
        // ==========================================

        /// <summary>
        /// Tickets en estado 'Pendiente' esperando asignación técnica.
        /// </summary>
        public int TicketPendientes { get; set; }

        /// <summary>
        /// Tickets en estado 'En Proceso' bajo revisión de un especialista.
        /// </summary>
        public int TicketEnProceso { get; set; }

        /// <summary>
        /// Tickets en estado 'Resuelto' completados con éxito.
        /// </summary>
        public int TicketResueltos { get; set; }

        /// <summary>
        /// Volumen total de incidentes históricos procesados por el centro de soporte.
        /// </summary>
        public int TotalTickets => TicketPendientes + TicketEnProceso + TicketResueltos;


        // ==========================================
        // 📦 RENDIMIENTO DE INVENTARIO
        // ==========================================

        /// <summary>
        /// Ranking ordenado de los artículos y servicios con mayor rotación comercial.
        /// </summary>
        public List<ProductoRanking> ProductosMasVendidos { get; set; } = new List<ProductoRanking>();
    }

    /// <summary>
    /// Estructura de transferencia de datos para representar la tracción de un producto en el mercado.
    /// </summary>
    public class ProductoRanking
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
        public bool EsServicio { get; set; }
    }
}