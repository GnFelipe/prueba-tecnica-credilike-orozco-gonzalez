using System.ComponentModel.DataAnnotations;

namespace Credilike.Core.DTOs
{
    public class ProcesarLiquidacionRequest
    {
        [Required(ErrorMessage = "El período es obligatorio.")]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$", ErrorMessage = "El formato del período debe ser YYYY-MM.")]
        public string Periodo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}
