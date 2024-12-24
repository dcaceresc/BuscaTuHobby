import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MenuDto } from '@app/core/models';
import { FaIconService, MenuService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';


@Component({
    selector: 'app-groups',
    imports: [ButtonComponent, RouterLink, TableComponent],
    templateUrl: './menus.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenusComponent implements OnInit {
  
  private menuService = inject(MenuService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<MenuDto[]>([]);
  public actions: any[] = [];
  
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'menuId' },
      { name: 'Nombre', key: 'menuName' },
      { name: 'Slug', key: 'menuSlug' },
      { name: 'SubMenus', key: 'subMenus' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]

    this.loadMenus();
  }

  public loadMenus() {
    this.menuService.getMenus().subscribe({
      next: (response) => {
        
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    })
  }

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/menus/edit', id]);
  }

  public onToggle(id: string) {
    this.menuService.toggleMenu(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadMenus();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onAction(event: { id: string, actionKey: string }) {
    switch (event.actionKey) {
      case 'edit':
        this.onEdit(event.id);
        break;
      case 'toggle':
        this.onToggle(event.id);
        break;
    }
  }

}
