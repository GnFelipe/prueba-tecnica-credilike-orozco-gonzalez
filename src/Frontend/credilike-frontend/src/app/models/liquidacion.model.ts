export type EstadoLiquidacion = 'Borrador' | 'EnProceso' | 'Aprobada' | 'Rechazada';

export interface LiquidacionDetalle {
  id: number;
  asesorId: number;
  nombreAsesor: string;
  montoVentas: number;
  montoComision: number;
  estado: string;
}

export interface Liquidacion {
  id: number;
  tenantId: number;
  periodo: string;
  montoTotal: number;
  totalAsesores: number;
  estado: EstadoLiquidacion;
  createdAt: string;
  createdBy: number;
  aprobadoPor?: number | null;
  fechaAprobacion?: string | null;
  observaciones?: string | null;
  detalles?: LiquidacionDetalle[];
}

export interface PagedResponse<T> {
  data: T[];
  totalRecords: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LiquidacionFilter {
  estado?: EstadoLiquidacion | 'Todas';
  periodo?: string;
  page: number;
  pageSize: number;
}
