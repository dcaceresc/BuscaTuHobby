import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { MenuComponent } from './menu.component';
import { DashboardService } from '@app/core/services';
import { NotificationService } from '@app/core/services/notification.service';

const dashboard = {
  getMenuCategories: vi.fn(),
  getMenuStores: vi.fn()
};
const notifications = { showError: vi.fn(), showDefaultError: vi.fn() };
const offcanvas = { dismiss: vi.fn() };

async function render() {
  TestBed.configureTestingModule({
    imports: [MenuComponent],
    providers: [
      provideRouter([]),
      { provide: DashboardService, useValue: dashboard },
      { provide: NotificationService, useValue: notifications },
      { provide: NgbActiveOffcanvas, useValue: offcanvas }
    ]
  });
  const fixture = TestBed.createComponent(MenuComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

function anchors(host: HTMLElement): HTMLAnchorElement[] {
  return Array.from(host.querySelectorAll('a'));
}

describe('MenuComponent', () => {
  beforeEach(() => {
    dashboard.getMenuCategories.mockReset();
    dashboard.getMenuStores.mockReset();
    notifications.showError.mockReset();
    notifications.showDefaultError.mockReset();
    dashboard.getMenuCategories.mockReturnValue(of({
      success: true,
      message: null,
      data: [{ categoryName: 'Gunpla', categoryIcon: 'bi-robot', categorySlug: 'gunpla' }]
    }));
    dashboard.getMenuStores.mockReturnValue(of({
      success: true,
      message: null,
      data: [{ storeName: 'Hobby Store', storeIcon: 'bi-shop', storeSlug: 'hobby-store' }]
    }));
  });

  it('keeps only the entries that resolve to a real route', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;
    const hrefs = anchors(host).map(anchor => anchor.getAttribute('href'));
    const text = host.textContent ?? '';

    expect(hrefs).toEqual(['/', null, null, '/login', '/register']);
    expect(text).toContain('Inicio');
    expect(text).toContain('Categorías');
    expect(text).toContain('Tiendas');
    expect(text).toContain('Iniciar Sesión');
    expect(text).toContain('Registrarse');
    expect(text).not.toContain('Ofertas');
    expect(text).not.toContain('Mis Favoritos');
    expect(text).not.toContain('Historial');
    expect(text).not.toContain('Ver todas');
    expect(host.querySelectorAll('a[href="#"]').length).toBe(0);
  });

  it('renders categories as non interactive rows instead of links', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;

    (Array.from(host.querySelectorAll('a')).find(anchor => anchor.textContent?.includes('Categorías')) as HTMLElement).click();
    fixture.detectChanges();

    const rows = Array.from(host.querySelectorAll('span.nav-link'));
    const row = rows.find(element => element.textContent?.includes('Gunpla'));

    expect(row).toBeDefined();
    expect(row?.querySelector('i')?.className).toContain('bi-robot');
    expect(anchors(host).some(anchor => anchor.getAttribute('href') === 'gunpla')).toBe(false);
    expect(ancestorsAreLinks(row)).toBe(false);
    expect(host.textContent).not.toContain('Ver todas');
  });

  it('renders stores as non interactive rows instead of links', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;

    (Array.from(host.querySelectorAll('a')).find(anchor => anchor.textContent?.includes('Tiendas')) as HTMLElement).click();
    fixture.detectChanges();

    const rows = Array.from(host.querySelectorAll('span.nav-link'));
    const row = rows.find(element => element.textContent?.includes('Hobby Store'));

    expect(row).toBeDefined();
    expect(row?.querySelector('i')?.className).toContain('bi-shop');
    expect(anchors(host).some(anchor => anchor.getAttribute('href') === 'hobby-store')).toBe(false);
    expect(ancestorsAreLinks(row)).toBe(false);
    expect(host.textContent).not.toContain('Ver todas');
  });

  it('closes the offcanvas from the home entry', async () => {
    const fixture = await render();
    const host = fixture.nativeElement as HTMLElement;
    const home = Array.from(host.querySelectorAll('a')).find(anchor => anchor.getAttribute('href') === '/');

    home?.click();

    expect(offcanvas.dismiss).toHaveBeenCalled();
  });
});

function ancestorsAreLinks(element: Element | undefined): boolean {
  let current = element?.parentElement ?? null;
  while (current) {
    if (current.tagName === 'A') {
      return true;
    }
    current = current.parentElement;
  }
  return false;
}
