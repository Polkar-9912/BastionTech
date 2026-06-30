using System;
using System.Collections.Generic;

namespace BastionTech.Models
{
    public class ReportesViewModel
    {
        // ==========================================
        // 📅 PARÁMETROS DE CONTROL TEMPORAL
        // ==========================================
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // ==========================================
        // 💰 MÉTRICAS HISTÓRICAS (FIJAS)
        // ==========================================
        public decimal IngresosTotales { get; set; }
        public int TotalOrdenes { get; set; }

        // ==========================================
        // 📈 MÉTRICAS DEL PERIODO FILTRADO (DINÁMICAS)
        // ==========================================
        public decimal IngresosPeriodo { get; set; }
        public List<Venta> VentasPeriodo { get; set; } = new List<Venta>();

        // ==========================================
        // 🛡️ MÉTRICAS OPERATIVAS GLOBALES (BLUE TEAM)
        // ==========================================
        public int TicketPendientes { get; set; }
        public int TicketEnProceso { get; set; }
        public int TicketResueltos { get; set; }
        public int TotalTickets => TicketPendientes + TicketEnProceso + TicketResueltos;

        // ==========================================
        // 📦 RENDIMIENTO DE INVENTARIO (TOP 5)
        // ==========================================
        public List<ProductoRanking> ProductosMasVendidos { get; set; } = new List<ProductoRanking>();
    }

    public class ProductoRanking
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
        public bool EsServicio { get; set; }
    }
}