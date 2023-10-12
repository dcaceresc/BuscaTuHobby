import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { universeVM } from 'src/app/core/models/universe.model';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { UniversesService } from 'src/app/core/services/universes.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink, FontAwesomeModule],
  templateUrl: './list-universes.component.html',
  styleUrls: ['./list-universes.component.scss']
})
export class ListUniversesComponent implements OnInit {
  universes!:universeVM[];
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;


  constructor(private universeService:UniversesService) { }
  
  ngOnInit(): void {
    this.universeService.GetAll().subscribe(items => this.universes = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.universes?.slice(startIndex,endIndex);
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
    return Math.ceil(this.universes?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const universe = this.universes.find(x => x.id === id);

    if(universe){
      this.universeService.Toggle(id).subscribe(
        () => {
          this.universeService.GetAll().subscribe(items => this.universes = items);
        }
      );
    }
  }
}
