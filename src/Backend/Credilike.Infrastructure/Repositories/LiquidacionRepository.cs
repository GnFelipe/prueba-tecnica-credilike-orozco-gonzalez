using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Credilike.Core.Entities;
using Credilike.Core.Interfaces;

namespace Credilike.Infrastructure.Repositories
{
    public class LiquidacionRepository : ILiquidacionRepository
    {
        // Simulación de almacén de datos persistente en memoria para demostración
        private static readonly List<Liquidacion> _liquidacionesStore = new List<Liquidacion>();

        public Task<Liquidacion?> GetByIdAsync(int id, int tenantId)
        {
            var item = _liquidacionesStore.FirstOrDefault(l => l.Id == id && l.TenantId == tenantId);
            return Task.FromResult(item);
        }

        public Task<bool> ExistsAprobadaForPeriodoAsync(int tenantId, string periodo)
        {
            bool exists = _liquidacionesStore.Any(l => 
                l.TenantId == tenantId && 
                l.Periodo == periodo && 
                l.Estado == EstadoLiquidacion.Aprobada);
            return Task.FromResult(exists);
        }

        public Task<List<LiquidacionDetalle>> GetVentasPendientesPorTenantAsync(int tenantId, string periodo)
        {
            // Datos de prueba simulados con multi-tenancy aislado
            var detallesMock = new List<LiquidacionDetalle>
            {
                new LiquidacionDetalle
                {
                    Id = 1,
                    TenantId = tenantId,
                    AsesorId = 101,
                    NombreAsesor = "Carlos Pérez",
                    MontoVentas = 15000000m,
                    MontoComision = 1500000m,
                    Estado = "Pendiente",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new LiquidacionDetalle
                {
                    Id = 2,
                    TenantId = tenantId,
                    AsesorId = 102,
                    NombreAsesor = "María Rodríguez",
                    MontoVentas = 22000000m,
                    MontoComision = 2200000m,
                    Estado = "Pendiente",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                }
            };

            return Task.FromResult(detallesMock);
        }

        public Task AddAsync(Liquidacion liquidacion)
        {
            if (liquidacion.Id <= 0)
            {
                liquidacion.Id = _liquidacionesStore.Count > 0 ? _liquidacionesStore.Max(l => l.Id) + 1 : 1001;
            }
            _liquidacionesStore.Add(liquidacion);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Liquidacion liquidacion)
        {
            var index = _liquidacionesStore.FindIndex(l => l.Id == liquidacion.Id && l.TenantId == liquidacion.TenantId);
            if (index >= 0)
            {
                _liquidacionesStore[index] = liquidacion;
            }
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
