import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { BestDealsComponent } from './best-deals.component';
import { DashboardService, NotificationService } from '@app/core/services';

const dashboard = { getBestDeals: vi.fn() };
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };

async function render() {
  TestBed.configureTestingModule({
    imports: [BestDealsComponent],
    providers: [
      provideRouter([]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications }
    ]
  });
  const fixture = TestBed.createComponent(BestDealsComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

const deal = (inventoryId: string, productId: string, name: string, discount: number) => ({
  inventoryId, productId, productName: name, storeName: 'Tienda', discountPercentage: discount, productViewCount: 10
});

describe('BestDealsComponent', () => {
  beforeEach(() => {
    dashboard.getBestDeals.mockReset();
    notifications.showError.mockReset();
    dashboard.getBestDeals.mockReturnValue(of({
      success: true, message: null, data: [deal('i1', 'p1', 'RX-78-2', 35), deal('i2', 'p2', 'Unicorn', 20)]
    }));
  });

  it('renders one row per highlighted offer with its discount', async () => {
    const fixture = await render();
    const rows = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr'));

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('RX-78-2');
    expect(rows[0].textContent).toContain('-35%');
    expect(rows[1].textContent).toContain('Unicorn');
  });

  it('links every highlighted offer to its product page', async () => {
    const fixture = await render();
    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('tbody a'));

    expect(links).toHaveLength(2);
    expect(links[0].getAttribute('href')).toBe('/products/p1');
    expect(links[1].getAttribute('href')).toBe('/products/p2');
  });

  it('reports a failed response instead of rendering empty rows', async () => {
    dashboard.getBestDeals.mockReturnValue(of({ success: false, message: 'No se pudo', data: [] }));
    const fixture = await render();

    expect(notifications.showError).toHaveBeenCalledWith('Error', 'No se pudo');
    expect((fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr')).toHaveLength(0);
  });
});
