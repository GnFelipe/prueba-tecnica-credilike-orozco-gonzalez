import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Liquidacion, LiquidacionFilter, PagedResponse } from '../models/liquidacion.model';

@Injectable({
  providedIn: 'root'
})
export class LiquidacionService {
  private readonly apiUrl = 'http://localhost:5000/api/liquidaciones';

  constructor(private http: HttpClient) {}

  /**
   * Obtiene la lista de liquidaciones paginada y filtrada por estado.
   */
  getLiquidaciones(filter: LiquidacionFilter): Observable<PagedResponse<Liquidacion>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.estado && filter.estado !== 'Todas') {
      params = params.set('estado', filter.estado);
    }

    if (filter.periodo) {
      params = params.set('periodo', filter.periodo);
    }

    return this.http.get<PagedResponse<Liquidacion>>(this.apiUrl, { params });
  }

  /**
   * Obtiene el detalle completo de una liquidación por su ID.
   */
  getLiquidacionById(id: number): Observable<Liquidacion> {
    return this.http.get<Liquidacion>(`${this.apiUrl}/${id}`);
  }

  /**
   * Aprueba una liquidación existente y actualiza su estado.
   */
  aprobarLiquidacion(id: number): Observable<Liquidacion> {
    return this.http.post<Liquidacion>(`${this.apiUrl}/${id}/aprobar`, {});
  }

  /**
   * Inicia el proceso de recálculo/liquidación de comisiones para un período.
   */
  procesarLiquidacion(periodo: string, observaciones?: string): Observable<Liquidacion> {
    return this.http.post<Liquidacion>(`${this.apiUrl}/procesar`, { periodo, observaciones });
  }
}
