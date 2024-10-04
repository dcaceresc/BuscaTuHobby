import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { Router, RouterLink } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { FaIconService } from '../../../core/services/fa-icon.service';
import { SubMenuService } from '../../../core/services/maintainer/submenu.service';
import { SubMenuDto } from '../../../core/models/maintainer/submenu.model';


@Component({
  selector: 'app-submenus',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,TableComponent,RouterLink
  ],
  templateUrl: './submenus.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScalesComponent implements OnInit { 

  private subMenuService = inject(SubMenuService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<SubMenuDto[]>([]);
  public actions: any[] = [];


  public ngOnInit() {
    this.columns = [
      { name: '#', key: 'subMenuId' },
      { name: 'Nombre', key: 'subMenuName' },
      { name: 'Menu', key: 'menuName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]

    this.loadScales();
  }

  public loadScales() {
    this.subMenuService.getSubMenus().subscribe({
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
    });
  }

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/submenus/update', id]);
  }

  public onToggle(id: string) {
    this.subMenuService.toggleSubMenu(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadScales();
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
