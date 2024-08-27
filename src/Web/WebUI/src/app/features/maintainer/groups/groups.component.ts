import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from '../../../shared/components/table/table.component';
import { GroupService } from '../../../core/services/maintainer/group.service';
import { GroupDto } from '../../../core/models/maintainer/group.model';

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

  public columns :any[] = [];
  public data = signal<GroupDto[]>([]);
  
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'groupId' },
      { name: 'Nombre', prop: 'groupName' },
      { name: 'Acciones', prop: 'IsActive' },
    ];
    this.loadGroups();
  }

  public loadGroups() {
    this.groupService.getGroups().subscribe({
      next: (response) => {
        
        if (!response.success) {
          console.error(response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: (error) => {
        console.error(error);
      },
    })
  }




}
