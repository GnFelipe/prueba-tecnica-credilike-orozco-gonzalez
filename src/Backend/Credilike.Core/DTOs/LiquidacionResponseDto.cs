using System;
using System.Collections.Generic;

namespace Credilike.Core.DTOs
{
    public class LiquidacionResponseDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int TotalAsesores { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public int? AprobadoPor { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? Observaciones { get; set; }

        public List<LiquidacionDetalleDto> Detalles { get; set; } = new List<LiquidacionDetalleDto>();
    }

    public class LiquidacionDetalleDto
    {
        public int Id { get; set; }
        public int AsesorId { get; set; }
        public string NombreAsesor { get; set; } = string.Empty;
        public decimal MontoVentas { get; set; }
        public decimal MontoComision { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
