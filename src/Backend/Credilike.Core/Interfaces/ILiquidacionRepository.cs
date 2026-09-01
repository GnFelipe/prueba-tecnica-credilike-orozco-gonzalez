using System.Collections.Generic;
using System.Threading.Tasks;
using Credilike.Core.Entities;

namespace Credilike.Core.Interfaces
{
    public interface ILiquidacionRepository
    {
        Task<Liquidacion?> GetByIdAsync(int id, int tenantId);
        Task<bool> ExistsAprobadaForPeriodoAsync(int tenantId, string periodo);
        Task<List<LiquidacionDetalle>> GetVentasPendientesPorTenantAsync(int tenantId, string periodo);
        Task AddAsync(Liquidacion liquidacion);
        Task UpdateAsync(Liquidacion liquidacion);
        Task SaveChangesAsync();
    }
}
