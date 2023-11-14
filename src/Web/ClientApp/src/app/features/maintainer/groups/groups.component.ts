import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupsService } from 'src/app/core/services/groups.service';
import { Router, RouterLink } from '@angular/router';
import { groupDto } from 'src/app/core/models/group.model';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './groups.component.html',
  styleUrl: './groups.component.scss'
})
export class GroupsComponent implements OnInit{
  groups = signal<groupDto[]>([]);
  columns:any[] = [];

  constructor(private groupsService:GroupsService, public router : Router) {}

  ngOnInit(): void {
    this.groupsService.GetAll().subscribe(items => this.groups.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id: number) {
    this.router.navigate(['/maintainer/franchises/edit/', id]);
  }

  onToggle(id: number) {
    const group = this.groups().find(x => x.id === id);

    if(group){
      this.groupsService.Toggle(id).subscribe(
        () => {
          this.groupsService.GetAll().subscribe(items => this.groups.set(items));
        }
      );
    }
  }
}
