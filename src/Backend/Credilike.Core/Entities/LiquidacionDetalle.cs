namespace Credilike.Core.Entities
{
    public class LiquidacionDetalle : BaseEntity
    {
        public int LiquidacionId { get; set; }
        public int AsesorId { get; set; }
        public string NombreAsesor { get; set; } = string.Empty;
        public decimal MontoVentas { get; set; }
        public decimal MontoComision { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public Liquidacion? Liquidacion { get; set; }
    }
}
