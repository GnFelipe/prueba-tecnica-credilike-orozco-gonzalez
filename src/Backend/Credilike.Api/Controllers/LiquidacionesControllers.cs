using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Credilike.Core.DTOs;
using Credilike.Core.Interfaces;

namespace Credilike.Api.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LiquidacionesController : ControllerBase
    {
        private readonly ILiquidacionService _liquidacionService;

        public LiquidacionesController(ILiquidacionService liquidacionService)
        {
            _liquidacionService = liquidacionService ?? throw new ArgumentNullException(nameof(liquidacionService));
        }

        /// <summary>
        /// POST /api/liquidaciones/procesar
        /// Inicia el cálculo de comisiones para el período indicado.
        /// Restricción: Solo rol Admin del tenant activo.
        /// </summary>
        [HttpPost("procesar")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LiquidacionResponseDto), 201)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> Procesar([FromBody] ProcesarLiquidacionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto(
                    "INVALID_MODEL_STATE",
                    "Los datos enviados en la solicitud no son válidos.",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                ));
            }

            int tenantId = GetTenantIdFromUser();
            int usuarioId = GetUserIdFromUser();

            var result = await _liquidacionService.ProcesarLiquidacionAsync(tenantId, usuarioId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// GET /api/liquidaciones/{id}
        /// Obtiene el detalle de una liquidación con sus líneas de comisión.
        /// Restricción: Admin y Supervisor del mismo tenant.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Supervisor")]
        [ProducesResponseType(typeof(LiquidacionResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> GetById(int id)
        {
            int tenantId = GetTenantIdFromUser();
            var result = await _liquidacionService.ObtenerLiquidacionPorIdAsync(tenantId, id);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/liquidaciones/{id}/aprobar
        /// Aprueba y marca lista para pago la liquidación.
        /// Restricción: Solo rol Aprobador del mismo tenant.
        /// </summary>
        [HttpPost("{id:int}/aprobar")]
        [Authorize(Roles = "Aprobador")]
        [ProducesResponseType(typeof(LiquidacionResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> Aprobar(int id)
        {
            int tenantId = GetTenantIdFromUser();
            int usuarioId = GetUserIdFromUser();

            var result = await _liquidacionService.AprobarLiquidacionAsync(tenantId, usuarioId, id);
            return Ok(result);
        }

        #region Helper Methods

        private int GetTenantIdFromUser()
        {
            // Extrae el TenantId del Claim del usuario autenticado JWT o Header X-Tenant-Id
            var tenantClaim = User.FindFirst("TenantId")?.Value;
            if (int.TryParse(tenantClaim, out int tenantId))
            {
                return tenantId;
            }

            if (Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) && int.TryParse(tenantHeader, out int headerTenantId))
            {
                return headerTenantId;
            }

            // Para entorno local o desarrollo por defecto
            return 1;
        }

        private int GetUserIdFromUser()
        {
            var userClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userClaim, out int userId))
            {
                return userId;
            }
            return 101; // ID de usuario por defecto en dev
        }

        #endregion
    }
}