import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { TableComponent } from '../../../../shared/components/table/table.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { Router, RouterLink } from '@angular/router';
import { RoleService } from '../../../../core/services/security/role.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { RoleDto } from '../../../../core/models/security/role.model';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    RouterLink,TableComponent,ButtonComponent
  ],
  templateUrl: './roles.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
      { icon: faEdit, label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: faPowerOff, label: '', actionKey: 'toggle', cssClass: '' },
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
    this.router.navigate(['/security/roles/update', roleId]);
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
