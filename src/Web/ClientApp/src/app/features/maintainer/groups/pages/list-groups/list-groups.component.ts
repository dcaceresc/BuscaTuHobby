import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { groupDto } from 'src/app/core/models/group.model';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { GroupsService } from 'src/app/core/services/groups.service';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-groups.component.html',
  styleUrls: ['./list-groups.component.scss']
})
export class ListGroupsComponent {
  groups = signal<groupDto[]>([]);
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private groupsService:GroupsService) {}

  ngOnInit(): void {
    this.groupsService.GetAll().subscribe(items => this.groups.set(items));
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.groups()?.slice(startIndex,endIndex);
  }

  setPage(pageNumber: number) {
    this.currentPage = pageNumber;
  }

  range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  nPage(){
    return Math.ceil(this.groups?.length / this.itemsPerPage)
  }

  toggle(id:number){
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