using System;

namespace Credilike.Core.Exceptions
{
    public class DomainException : Exception
    {
        public string Codigo { get; }
        public string Detalle { get; }

        public DomainException(string codigo, string mensaje, string detalle)
            : base(mensaje)
        {
            Codigo = codigo;
            Detalle = detalle;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string recurso, object id)
            : base("RESOURCE_NOT_FOUND", $"El recurso '{recurso}' con id '{id}' no fue encontrado.", "Verifique el identificador proporcionado.")
        { }
    }

    public class TenantForbiddenException : DomainException
    {
        public TenantForbiddenException(int tenantSolicitante, int tenantRecurso)
            : base("TENANT_ACCESS_DENIED", "Acceso denegado a datos de otro tenant.", $"El tenant solicitante ({tenantSolicitante}) no tiene permisos para acceder o modificar los datos del tenant ({tenantRecurso}).")
        { }
    }

    public class InvalidOperationDomainException : DomainException
    {
        public InvalidOperationDomainException(string codigo, string mensaje, string detalle)
            : base(codigo, mensaje, detalle)
        { }
    }
}
