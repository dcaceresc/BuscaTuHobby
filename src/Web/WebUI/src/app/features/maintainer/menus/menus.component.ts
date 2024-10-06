import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from '../../../shared/components/table/table.component';
import { MenuService } from '../../../core/services/maintainer/menu.service';
import { MenuDto } from '../../../core/models/maintainer/menu.model';
import { NotificationService } from '../../../core/services/notification.service';
import { FaIconService } from '../../../core/services/fa-icon.service';

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,RouterLink,TableComponent
  ],
  templateUrl: './menus.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
    this.router.navigate(['/maintainer/menus/update', id]);
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