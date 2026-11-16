import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { describe, expect, it } from 'vitest';
import { NotFoundComponent } from './not-found.component';
import { routes } from '../../app.routes';
import { UserLoginComponent } from '../security/auth/user-login/user-login.component';
import { RegisterComponent } from '../security/auth/register/register.component';

async function renderNotFound(): Promise<HTMLElement> {
  TestBed.configureTestingModule({
    imports: [NotFoundComponent],
    providers: [provideRouter([])]
  });
  const fixture = TestBed.createComponent(NotFoundComponent);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

function configureAppRoutes(): void {
  TestBed.configureTestingModule({
    providers: [
      provideRouter(routes),
      provideHttpClient(),
      provideHttpClientTesting()
    ]
  });
}

describe('NotFoundComponent', () => {
  it('explains the missing page in Spanish and links back home', async () => {
    const host = await renderNotFound();
    const text = host.textContent ?? '';
    const home = host.querySelector('a');

    expect(text).toContain('404');
    expect(text).toContain('La página que buscas no existe');
    expect(home?.getAttribute('href')).toBe('/');
    expect(home?.textContent).toContain('Ir a la página de inicio');
  });

  it('answers an unknown URL instead of breaking navigation', async () => {
    configureAppRoutes();
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl('/una-ruta-que-no-existe');

    expect(harness.routeNativeElement?.querySelector('app-not-found')).not.toBeNull();
    expect(harness.routeNativeElement?.textContent).toContain('La página que buscas no existe');
  });

  it('keeps the login and register pages reachable next to the wildcard route', async () => {
    configureAppRoutes();
    const harness = await RouterTestingHarness.create();

    const login = await harness.navigateByUrl('/login', UserLoginComponent);
    const register = await harness.navigateByUrl('/register', RegisterComponent);

    expect(login).toBeInstanceOf(UserLoginComponent);
    expect(register).toBeInstanceOf(RegisterComponent);
  });
});
