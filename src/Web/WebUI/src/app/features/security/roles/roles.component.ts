import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { RoleDto } from '@app/core/models';
import { NotificationService, RoleService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-roles',
    imports: [RouterLink, TableComponent, ButtonComponent],
    templateUrl: './roles.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RolesComponent implements OnInit {

  private router = inject(Router);
  private roleService = inject(RoleService);
  private notificationService = inject(NotificationService);

  public columns : any[] = [];
  public data = signal<RoleDto[]>([]);
  public actions : any[] = [];


  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'roleId' },
      { name: 'Nombre', key: 'roleName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-power-off', label: '', actionKey: 'toggle', cssClass: '' },
    ]

    this.loadRoles();
  }

  public loadRoles() {
    this.roleService.getRoles().subscribe({
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

  public onEdit(roleId: string) {
    this.router.navigate(['/security/roles/edit', roleId]);
  }

  public onToggle(roleId: string) {
    this.roleService.toggleRole(roleId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.loadRoles();
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

}
