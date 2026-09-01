import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LiquidacionService } from './liquidacion.service';
import { Liquidacion, LiquidacionFilter, PagedResponse } from '../models/liquidacion.model';

describe('LiquidacionService', () => {
  let service: LiquidacionService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LiquidacionService]
    });
    service = TestBed.inject(LiquidacionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('debe crearse correctamente el servicio', () => {
    expect(service).toBeTruthy();
  });

  it('debe solicitar liquidaciones paginadas con filtros correctos', () => {
    const mockFilter: LiquidacionFilter = { page: 1, pageSize: 10, estado: 'Borrador' };
    const mockResponse: PagedResponse<Liquidacion> = {
      data: [
        {
          id: 101,
          tenantId: 1,
          periodo: '2026-08',
          montoTotal: 1500000,
          totalAsesores: 3,
          estado: 'Borrador',
          createdAt: '2026-09-01T00:00:00Z',
          createdBy: 10
        }
      ],
      totalRecords: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1
    };

    service.getLiquidaciones(mockFilter).subscribe((response) => {
      expect(response.data.length).toBe(1);
      expect(response.data[0].estado).toBe('Borrador');
      expect(response.data[0].periodo).toBe('2026-08');
    });

    const req = httpMock.expectOne((request) => 
      request.url === 'http://localhost:5000/api/liquidaciones' &&
      request.params.get('page') === '1' &&
      request.params.get('pageSize') === '10' &&
      request.params.get('estado') === 'Borrador'
    );

    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('debe enviar la solicitud de aprobación correctamente vía POST', () => {
    const liquidacionId = 101;
    const mockLiquidacionAprobada: Liquidacion = {
      id: liquidacionId,
      tenantId: 1,
      periodo: '2026-08',
      montoTotal: 1500000,
      totalAsesores: 3,
      estado: 'Aprobada',
      createdAt: '2026-09-01T00:00:00Z',
      createdBy: 10,
      aprobadoPor: 99,
      fechaAprobacion: '2026-09-01T12:00:00Z'
    };

    service.aprobarLiquidacion(liquidacionId).subscribe((res) => {
      expect(res.estado).toBe('Aprobada');
      expect(res.aprobadoPor).toBe(99);
    });

    const req = httpMock.expectOne(`http://localhost:5000/api/liquidaciones/${liquidacionId}/aprobar`);
    expect(req.request.method).toBe('POST');
    req.flush(mockLiquidacionAprobada);
  });
});
