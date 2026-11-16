import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { LoginMenuComponent } from './login-menu.component';
import { AuthService } from '@app/core/services';

const auth = {
  isAuthenticated: vi.fn(),
  logout: vi.fn()
};

async function render(authenticated: boolean) {
  auth.isAuthenticated.mockReturnValue(of(authenticated));

  TestBed.configureTestingModule({
    imports: [LoginMenuComponent],
    providers: [
      provideRouter([]),
      { provide: AuthService, useValue: auth }
    ]
  });
  const fixture = TestBed.createComponent(LoginMenuComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture;
}

describe('LoginMenuComponent', () => {
  beforeEach(() => {
    auth.isAuthenticated.mockReset();
    auth.logout.mockReset();
  });

  it('offers login and register to anonymous visitors', async () => {
    const fixture = await render(false);
    const host = fixture.nativeElement as HTMLElement;
    const text = host.textContent ?? '';

    expect(text).toContain('Iniciar Sesión');
    expect(text).toContain('Registrarse');
    expect(text).not.toContain('Cerrar Sesión');
  });

  it('shows no profile entry because no profile route exists', async () => {
    const fixture = await render(true);
    const host = fixture.nativeElement as HTMLElement;
    const text = host.textContent ?? '';

    expect(text).not.toContain('Perfil');
    expect(host.querySelector('a[href="/profile"]')).toBeNull();
    expect(text).toContain('Cerrar Sesión');
  });

  it('logs out from the authenticated menu', async () => {
    const fixture = await render(true);
    const host = fixture.nativeElement as HTMLElement;
    const logout = Array.from(host.querySelectorAll('button')).find(button => button.textContent?.includes('Cerrar Sesión'));

    logout?.click();

    expect(auth.logout).toHaveBeenCalledTimes(1);
  });
});
