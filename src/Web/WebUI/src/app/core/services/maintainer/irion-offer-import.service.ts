import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiResponse, ConfirmIrionOffer, ConfirmNewIrionProductOffer, InspectNewIrionProductOffer, NewIrionProductOptionsDto, IrionOfferConfirmationDto, IrionOfferInspectionDto, IrionOfferSelection } from '@app/core/models';

@Injectable({ providedIn: 'root' })
export class IrionOfferImportService {
  private readonly http = inject(HttpClient);

  getNewProductOptions() {
    return this.http.get<ApiResponse<NewIrionProductOptionsDto | null>>(
      '/api/maintainer/inventories/irion/new-product/options');
  }

  inspectNewProduct(request: InspectNewIrionProductOffer) {
    return this.http.post<ApiResponse<IrionOfferInspectionDto | null>>(
      '/api/maintainer/inventories/irion/new-product/inspect', request);
  }

  confirmNewProduct(request: ConfirmNewIrionProductOffer) {
    return this.http.post<ApiResponse<IrionOfferConfirmationDto | null>>(
      '/api/maintainer/inventories/irion/new-product/confirm', request);
  }

  inspect(selection: IrionOfferSelection) {
    return this.http.post<ApiResponse<IrionOfferInspectionDto | null>>(
      '/api/maintainer/inventories/irion/inspect', selection);
  }

  confirm(request: ConfirmIrionOffer) {
    return this.http.post<ApiResponse<IrionOfferConfirmationDto | null>>(
      '/api/maintainer/inventories/irion/confirm', request);
  }
}
