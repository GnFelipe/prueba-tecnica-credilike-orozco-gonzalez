using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Credilike.Core.DTOs;
using Credilike.Core.Entities;
using Credilike.Core.Exceptions;
using Credilike.Core.Interfaces;

namespace Credilike.Core.Services
{
    public class LiquidacionService : ILiquidacionService
    {
        private readonly ILiquidacionRepository _repository;

        public LiquidacionService(ILiquidacionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<LiquidacionResponseDto> ProcesarLiquidacionAsync(int tenantId, int usuarioId, ProcesarLiquidacionRequest request)
        {
            if (tenantId <= 0)
                throw new ArgumentException("El TenantId debe ser un identificador válido.", nameof(tenantId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validar que no exista una liquidación aprobada para el período
            bool existeAprobada = await _repository.ExistsAprobadaForPeriodoAsync(tenantId, request.Periodo);
            if (existeAprobada)
            {
                throw new InvalidOperationDomainException(
                    "LIQUIDACION_PERIODO_YA_APROBADO",
                    $"Ya existe una liquidación aprobada para el período '{request.Periodo}'.",
                    "No se permite reliquidar períodos que ya han sido finalizados y aprobados."
                );
            }

            // Obtener ventas o comisiones pendientes del tenant
            List<LiquidacionDetalle> detallesCalculados = await _repository.GetVentasPendientesPorTenantAsync(tenantId, request.Periodo);
            
            if (detallesCalculados == null || !detallesCalculados.Any())
            {
                throw new InvalidOperationDomainException(
                    "SIN_VENTAS_PARA_LIQUIDAR",
                    $"No se encontraron ventas pendientes de liquidar para el período '{request.Periodo}'.",
                    "Asegúrese de tener registros de ventas cerrados en el rango seleccionado."
                );
            }

            decimal montoTotal = detallesCalculados.Sum(d => d.MontoComision);
            int totalAsesores = detallesCalculados.Select(d => d.AsesorId).Distinct().Count();

            var liquidacion = new Liquidacion
            {
                TenantId = tenantId,
                Periodo = request.Periodo,
                MontoTotal = montoTotal,
                TotalAsesores = totalAsesores,
                Estado = EstadoLiquidacion.Borrador,
                Observaciones = request.Observaciones,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioId,
                Detalles = detallesCalculados
            };

            await _repository.AddAsync(liquidacion);
            await _repository.SaveChangesAsync();

            return MapToResponseDto(liquidacion);
        }

        public async Task<LiquidacionResponseDto> ObtenerLiquidacionPorIdAsync(int tenantId, int id)
        {
            var liquidacion = await _repository.GetByIdAsync(id, tenantId);
            if (liquidacion == null)
            {
                throw new NotFoundException("Liquidación", id);
            }

            // Aislamiento Multi-Tenant explícito
            if (liquidacion.TenantId != tenantId)
            {
                throw new TenantForbiddenException(tenantId, liquidacion.TenantId);
            }

            return MapToResponseDto(liquidacion);
        }

        public async Task<LiquidacionResponseDto> AprobarLiquidacionAsync(int tenantId, int usuarioId, int id)
        {
            var liquidacion = await _repository.GetByIdAsync(id, tenantId);
            if (liquidacion == null)
            {
                throw new NotFoundException("Liquidación", id);
            }

            if (liquidacion.TenantId != tenantId)
            {
                throw new TenantForbiddenException(tenantId, liquidacion.TenantId);
            }

            if (liquidacion.Estado == EstadoLiquidacion.Aprobada)
            {
                throw new InvalidOperationDomainException(
                    "LIQUIDACION_YA_APROBADA",
                    "La liquidación seleccionada ya fue aprobada previamente.",
                    $"Fue aprobada el {liquidacion.FechaAprobacion:yyyy-MM-dd HH:mm} por el usuario ID {liquidacion.AprobadoPor}."
                );
            }

            if (liquidacion.Estado == EstadoLiquidacion.Rechazada)
            {
                throw new InvalidOperationDomainException(
                    "LIQUIDACION_RECHAZADA",
                    "No se puede aprobar una liquidación que se encuentra en estado Rechazada.",
                    "Genere una nueva liquidación en borrador para este período."
                );
            }

            liquidacion.Estado = EstadoLiquidacion.Aprobada;
            liquidacion.AprobadoPor = usuarioId;
            liquidacion.FechaAprobacion = DateTime.UtcNow;

            foreach (var detalle in liquidacion.Detalles)
            {
                detalle.Estado = "ListaParaPago";
            }

            await _repository.UpdateAsync(liquidacion);
            await _repository.SaveChangesAsync();

            return MapToResponseDto(liquidacion);
        }

        private static LiquidacionResponseDto MapToResponseDto(Liquidacion entity)
        {
            return new LiquidacionResponseDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Periodo = entity.Periodo,
                MontoTotal = entity.MontoTotal,
                TotalAsesores = entity.TotalAsesores,
                Estado = entity.Estado.ToString(),
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                AprobadoPor = entity.AprobadoPor,
                FechaAprobacion = entity.FechaAprobacion,
                Observaciones = entity.Observaciones,
                Detalles = entity.Detalles.Select(d => new LiquidacionDetalleDto
                {
                    Id = d.Id,
                    AsesorId = d.AsesorId,
                    NombreAsesor = d.NombreAsesor,
                    MontoVentas = d.MontoVentas,
                    MontoComision = d.MontoComision,
                    Estado = d.Estado
                }).ToList()
            };
        }
    }
}
