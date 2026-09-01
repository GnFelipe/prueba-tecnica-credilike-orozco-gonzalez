using System;
using System.Collections.Generic;

namespace Credilike.Core.Entities
{
    public enum EstadoLiquidacion
    {
        Borrador = 1,
        EnProceso = 2,
        Aprobada = 3,
        Rechazada = 4
    }

    public class Liquidacion : BaseEntity
    {
        public string Periodo { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int TotalAsesores { get; set; }
        public EstadoLiquidacion Estado { get; set; } = EstadoLiquidacion.Borrador;
        public int? AprobadoPor { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? Observaciones { get; set; }

        public ICollection<LiquidacionDetalle> Detalles { get; set; } = new List<LiquidacionDetalle>();
    }
}
