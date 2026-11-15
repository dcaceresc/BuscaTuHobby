import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiResponse, IrionProductPreviewDto } from '@app/core/models';

@Injectable({ providedIn: 'root' })
export class IrionProductPreviewService {
  private readonly http = inject(HttpClient);

  preview(url: string) {
    return this.http.post<ApiResponse<IrionProductPreviewDto>>(
      '/api/maintainer/products/preview/irion', { url });
  }
}
