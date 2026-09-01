using System;

namespace Credilike.Core.Entities
{
    /// <summary>
    /// Entidad base que exige los atributos mínimos requeridos por la rúbrica
    /// para aislamiento multi-tenant y auditoría básica.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
    }
}
