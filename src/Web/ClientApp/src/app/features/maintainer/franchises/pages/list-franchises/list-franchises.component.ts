import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { franchiseDto } from 'src/app/core/models/franchise.model';
import { FranchisesService } from 'src/app/core/services/franchises.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-franchises.component.html',
  styleUrls: ['./list-franchises.component.scss']
})
export class ListFranchisesComponent {
  franchises!:franchiseDto[];
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private franchisesService:FranchisesService) {}

  ngOnInit(): void {
    this.franchisesService.GetAll().subscribe(items => this.franchises = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.franchises?.slice(startIndex,endIndex);
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
    return Math.ceil(this.franchises?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const group = this.franchises.find(x => x.id === id);

    if(group){
      this.franchisesService.Toggle(id).subscribe(
        () => {
          this.franchisesService.GetAll().subscribe(items => this.franchises = items);
        }
      );
    }
  }
}