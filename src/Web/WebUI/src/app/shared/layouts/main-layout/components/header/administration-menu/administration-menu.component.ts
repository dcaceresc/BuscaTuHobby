
import { ChangeDetectionStrategy, Component, ElementRef, inject, input, viewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '@app/core/services/security/auth.service';
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';

@Component({
    selector: 'app-administration-menu',
    imports: [
    RouterLink,
    RouterLinkActive,
],
    templateUrl: './administration-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdministrationMenuComponent { 

  private authService = inject(AuthService);
  activeOffcanvas = inject(NgbActiveOffcanvas);

  public maintanerLinks = [
    { label: 'Categorias', link: '/maintainer/categories', icon : 'bi bi-tags' },
    { label: 'Comunas', link: '/maintainer/communes', icon : 'bi bi-geo-alt' },
    { label: 'Configuraciones', link: '/maintainer/configurations', icon : 'bi bi-gear' },
    { label: 'Menus', link: '/maintainer/menus', icon : 'bi bi-list' },
    { label: 'Franquicias', link: '/maintainer/franchises', icon : 'bi bi-building' },
    { label: 'Productos', link: '/maintainer/products', icon : 'bi bi-box-seam' },
    { label: 'Fabricantes', link: '/maintainer/manufacturers', icon : 'bi bi-tools' },
    { label: 'Inventario', link: '/maintainer/inventories', icon : 'bi bi-box-seam' },
    { label: 'Regiones', link: '/maintainer/regions', icon : 'bi bi-geo-alt' },
    { label: 'Series', link: '/maintainer/series', icon : 'bi bi-tags' },
    { label: 'Tiendas', link: '/maintainer/stores', icon : 'bi bi-building' },
  ]

  isSuperAdmin() {
    return this.authService.isSuperAdmin();
  }
}
