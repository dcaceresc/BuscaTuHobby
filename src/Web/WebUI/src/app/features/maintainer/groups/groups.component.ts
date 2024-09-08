import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from '../../../shared/components/table/table.component';
import { GroupService } from '../../../core/services/maintainer/group.service';
import { GroupDto } from '../../../core/models/maintainer/group.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [
    CommonModule,ButtonComponent,RouterLink,TableComponent
  ],
  templateUrl: './groups.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GroupsComponent implements OnInit {
  
  private groupService = inject(GroupService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  public columns :any[] = [];
  public data = signal<GroupDto[]>([]);
  
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'groupId' },
      { name: 'Nombre', key: 'groupName' },
      { name: 'Acciones', key: 'isActive' },
    ];
    this.loadGroups();
  }

  public loadGroups() {
    this.groupService.getGroups().subscribe({
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
    this.router.navigate(['/maintainer/groups/update', id]);
  }

  public onToggle(id: string) {
    this.groupService.toggleGroup(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadGroups();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }




}
