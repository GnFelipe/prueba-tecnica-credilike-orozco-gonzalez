using System.Threading.Tasks;
using Credilike.Core.DTOs;

namespace Credilike.Core.Interfaces
{
    public interface ILiquidacionService
    {
        Task<LiquidacionResponseDto> ProcesarLiquidacionAsync(int tenantId, int usuarioId, ProcesarLiquidacionRequest request);
        Task<LiquidacionResponseDto> ObtenerLiquidacionPorIdAsync(int tenantId, int id);
        Task<LiquidacionResponseDto> AprobarLiquidacionAsync(int tenantId, int usuarioId, int id);
    }
}
