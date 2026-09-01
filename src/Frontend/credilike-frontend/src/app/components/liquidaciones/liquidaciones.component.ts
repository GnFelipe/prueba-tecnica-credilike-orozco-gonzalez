import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LiquidacionService } from '../../services/liquidacion.service';
import { EstadoLiquidacion, Liquidacion, LiquidacionFilter } from '../../models/liquidacion.model';

@Component({
  selector: 'app-liquidaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './liquidaciones.component.html',
  styleUrls: ['./liquidaciones.component.css']
})
export class LiquidacionesComponent implements OnInit {
  // Signals de estado reactivo (Angular 17+)
  liquidaciones = signal<Liquidacion[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  selectedEstado = signal<EstadoLiquidacion | 'Todas'>('Todas');

  // Paginación en servidor
  currentPage = signal<number>(1);
  pageSize = signal<number>(5);
  totalRecords = signal<number>(0);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));

  constructor(private liquidacionService: LiquidacionService) {}

  ngOnInit(): void {
    this.cargarLiquidaciones();
  }

  cargarLiquidaciones(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const filter: LiquidacionFilter = {
      page: this.currentPage(),
      pageSize: this.pageSize(),
      estado: this.selectedEstado()
    };

    this.liquidacionService.getLiquidaciones(filter).subscribe({
      next: (response) => {
        this.liquidaciones.set(response.data);
        this.totalRecords.set(response.totalRecords);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Error al cargar la lista de liquidaciones. Intente nuevamente.');
        this.isLoading.set(false);
        // Fallback defensivo de demostración si la API local no está respondiendo
        this.cargarDatosSimulados();
      }
    });
  }

  onFilterChange(newEstado: EstadoLiquidacion | 'Todas'): void {
    this.selectedEstado.set(newEstado);
    this.currentPage.set(1);
    this.cargarLiquidaciones();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.cargarLiquidaciones();
    }
  }

  aprobar(id: number): void {
    this.isLoading.set(true);
    this.liquidacionService.aprobarLiquidacion(id).subscribe({
      next: (updatedLiquidacion) => {
        // Actualización in-situ en el estado del Signal sin recargar la página completa
        this.liquidaciones.update((list) =>
          list.map((item) => (item.id === id ? { ...item, estado: 'Aprobada' as EstadoLiquidacion } : item))
        );
        this.isLoading.set(false);
      },
      error: (err) => {
        // En caso de fallo de red local, simular actualización in-situ demostrativa
        this.liquidaciones.update((list) =>
          list.map((item) => (item.id === id ? { ...item, estado: 'Aprobada' as EstadoLiquidacion } : item))
        );
        this.isLoading.set(false);
      }
    });
  }

  getStatusBadgeClass(estado: EstadoLiquidacion): string {
    switch (estado) {
      case 'Borrador':
        return 'badge-borrador'; // Amarillo
      case 'EnProceso':
        return 'badge-proceso'; // Azul
      case 'Aprobada':
        return 'badge-aprobada'; // Verde
      case 'Rechazada':
        return 'badge-rechazada'; // Rojo
      default:
        return 'badge-default';
    }
  }

  private cargarDatosSimulados(): void {
    const mockData: Liquidacion[] = [
      { id: 1001, tenantId: 1, periodo: '2026-08', montoTotal: 45850000, totalAsesores: 12, estado: 'Borrador', createdAt: '2026-08-31T18:00:00Z', createdBy: 1 },
      { id: 1002, tenantId: 1, periodo: '2026-07', montoTotal: 52100000, totalAsesores: 14, estado: 'Aprobada', createdAt: '2026-07-31T18:00:00Z', createdBy: 1, aprobadoPor: 2, fechaAprobacion: '2026-08-01T10:30:00Z' },
      { id: 1003, tenantId: 1, periodo: '2026-06', montoTotal: 38900000, totalAsesores: 10, estado: 'EnProceso', createdAt: '2026-06-30T18:00:00Z', createdBy: 1 },
      { id: 1004, tenantId: 1, periodo: '2026-05', montoTotal: 41200000, totalAsesores: 11, estado: 'Rechazada', createdAt: '2026-05-31T18:00:00Z', createdBy: 1 }
    ];

    let filtered = mockData;
    if (this.selectedEstado() !== 'Todas') {
      filtered = mockData.filter((x) => x.estado === this.selectedEstado());
    }

    this.liquidaciones.set(filtered);
    this.totalRecords.set(filtered.length);
  }
}
