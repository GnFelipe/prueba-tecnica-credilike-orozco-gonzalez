using System;

namespace Credilike.Core.DTOs
{
    /// <summary>
    /// Estructura estándar de respuesta de error requerida por la rúbrica backend (código, mensaje, detalle).
    /// </summary>
    public class ErrorResponseDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ErrorResponseDto() { }

        public ErrorResponseDto(string codigo, string mensaje, string detalle)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Detalle = detalle;
        }
    }
}
