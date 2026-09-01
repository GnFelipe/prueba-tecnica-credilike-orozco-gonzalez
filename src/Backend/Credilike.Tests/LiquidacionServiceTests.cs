using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Credilike.Core.DTOs;
using Credilike.Core.Entities;
using Credilike.Core.Exceptions;
using Credilike.Core.Interfaces;
using Credilike.Core.Services;

namespace Credilike.Tests
{
    public class LiquidacionServiceTests
    {
        private readonly Mock<ILiquidacionRepository> _repositoryMock;
        private readonly LiquidacionService _service;

        public LiquidacionServiceTests()
        {
            _repositoryMock = new Mock<ILiquidacionRepository>();
            _service = new LiquidacionService(_repositoryMock.Object);
        }

        [Fact]
        public async Task ProcesarLiquidacionAsync_Exitoso_CreaLiquidacionEnEstadoBorrador()
        {
            // Arrange
            int tenantId = 10;
            int usuarioId = 5;
            var request = new ProcesarLiquidacionRequest { Periodo = "2026-08", Observaciones = "Test automatizado" };

            _repositoryMock.Setup(r => r.ExistsAprobadaForPeriodoAsync(tenantId, "2026-08"))
                .ReturnsAsync(false);

            var detallesPendientes = new List<LiquidacionDetalle>
            {
                new LiquidacionDetalle { Id = 1, TenantId = tenantId, AsesorId = 101, MontoComision = 500000m },
                new LiquidacionDetalle { Id = 2, TenantId = tenantId, AsesorId = 102, MontoComision = 750000m }
            };

            _repositoryMock.Setup(r => r.GetVentasPendientesPorTenantAsync(tenantId, "2026-08"))
                .ReturnsAsync(detallesPendientes);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Liquidacion>()))
                .Returns(Task.CompletedTask);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ProcesarLiquidacionAsync(tenantId, usuarioId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Borrador", result.Estado);
            Assert.Equal(1250000m, result.MontoTotal);
            Assert.Equal(2, result.TotalAsesores);
            Assert.Equal(tenantId, result.TenantId);
            Assert.Equal(usuarioId, result.CreatedBy);

            _repositoryMock.Verify(r => r.AddAsync(It.Is<Liquidacion>(l => 
                l.Estado == EstadoLiquidacion.Borrador && 
                l.MontoTotal == 1250000m &&
                l.TenantId == tenantId
            )), Times.Once);
        }

        [Fact]
        public async Task ProcesarLiquidacionAsync_PeriodoYaAprobado_LanzaInvalidOperationDomainException()
        {
            // Arrange
            int tenantId = 10;
            int usuarioId = 5;
            var request = new ProcesarLiquidacionRequest { Periodo = "2026-08" };

            _repositoryMock.Setup(r => r.ExistsAprobadaForPeriodoAsync(tenantId, "2026-08"))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationDomainException>(() => 
                _service.ProcesarLiquidacionAsync(tenantId, usuarioId, request)
            );

            Assert.Equal("LIQUIDACION_PERIODO_YA_APROBADO", exception.Codigo);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Liquidacion>()), Times.Never);
        }

        [Fact]
        public async Task ObtenerLiquidacionPorIdAsync_TenantDiferente_LanzaTenantForbiddenException()
        {
            // Arrange
            int tenantSolicitante = 10;
            int tenantDuenio = 99;
            int liquidacionId = 50;

            var liquidacionOtroTenant = new Liquidacion
            {
                Id = liquidacionId,
                TenantId = tenantDuenio,
                Periodo = "2026-08",
                Estado = EstadoLiquidacion.Borrador
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(liquidacionId, tenantSolicitante))
                .ReturnsAsync(liquidacionOtroTenant);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TenantForbiddenException>(() => 
                _service.ObtenerLiquidacionPorIdAsync(tenantSolicitante, liquidacionId)
            );

            Assert.Equal("TENANT_ACCESS_DENIED", exception.Codigo);
        }

        [Fact]
        public async Task AprobarLiquidacionAsync_EstadoValido_CambiaEstadoAAprobadaYAsignaAuditoria()
        {
            // Arrange
            int tenantId = 10;
            int usuarioAprobadorId = 88;
            int liquidacionId = 50;

            var liquidacionBorrador = new Liquidacion
            {
                Id = liquidacionId,
                TenantId = tenantId,
                Periodo = "2026-08",
                Estado = EstadoLiquidacion.Borrador,
                Detalles = new List<LiquidacionDetalle>
                {
                    new LiquidacionDetalle { Id = 1, TenantId = tenantId, Estado = "Pendiente" }
                }
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(liquidacionId, tenantId))
                .ReturnsAsync(liquidacionBorrador);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Liquidacion>()))
                .Returns(Task.CompletedTask);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AprobarLiquidacionAsync(tenantId, usuarioAprobadorId, liquidacionId);

            // Assert
            Assert.Equal("Aprobada", result.Estado);
            Assert.Equal(usuarioAprobadorId, result.AprobadoPor);
            Assert.NotNull(result.FechaAprobacion);

            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Liquidacion>(l => 
                l.Estado == EstadoLiquidacion.Aprobada && 
                l.AprobadoPor == usuarioAprobadorId
            )), Times.Once);
        }
    }
}
