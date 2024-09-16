import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { TableComponent } from '../../../../shared/components/table/table.component';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../../../../core/services/security/user.service';
import { UserDto } from '../../../../core/models/security/user.model';
import { NotificationService } from '../../../../core/services/notification.service';
import { faEdit, faPowerOff, faRotate } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    RouterLink,ButtonComponent,TableComponent,FontAwesomeModule
  ],
  templateUrl: './users.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent implements OnInit {

  private router = inject(Router);
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);

  public columns : any[] = [];
  public data = signal<UserDto[]>([]);
  public actions : any[] = [];

  public ngOnInit() {

    this.columns = [
      { name: '#', key: 'userId' },
      { name: 'Email', key: 'email' },
      { name: 'Email Confirmed', key: 'emailConfirmed' },
      { name: 'Lockout Enabled', key: 'lockoutEnabled' },
      { name: 'Lockout End', key: 'lockoutEnd' },
      { name: 'Roles', key: 'roleNames' },
      { name: 'Acciones', key: 'isActive' },
    ];



    this.actions = [
      { icon: faEdit, label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: faPowerOff, label: '', actionKey: 'toggle', cssClass: '' },
      { icon: faRotate, label: 'Reiniciar Contraseña', actionKey: 'reset', cssClass: 'bg-info' },
    ]

    this.loadUsers();
  }

  public loadUsers() {
    this.userService.getUsers().subscribe({
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


  public onEdit(userId: string) {
    this.router.navigate(['/security/users/update', userId]);
  }

  public onToggle(userId: string) {
    this.userService.toggleUser(userId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.loadUsers();
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
