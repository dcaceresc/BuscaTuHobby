import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiResponse, ConfirmIrionOffer, IrionOfferConfirmationDto, IrionOfferInspectionDto, IrionOfferSelection } from '@app/core/models';

@Injectable({ providedIn: 'root' })
export class IrionOfferImportService {
  private readonly http = inject(HttpClient);

  inspect(selection: IrionOfferSelection) {
    return this.http.post<ApiResponse<IrionOfferInspectionDto | null>>(
      '/api/maintainer/inventories/irion/inspect', selection);
  }

  confirm(request: ConfirmIrionOffer) {
    return this.http.post<ApiResponse<IrionOfferConfirmationDto | null>>(
      '/api/maintainer/inventories/irion/confirm', request);
  }
}
