import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { scaleDto } from 'src/app/core/models/scale.model';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { ScalesService } from 'src/app/core/services/scales.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-scales.component.html',
  styleUrls: ['./list-scales.component.scss']
})
export class ListScalesComponent {
  scales!:scaleDto[];
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private scaleService:ScalesService) {}

  ngOnInit(): void {
    this.scaleService.GetAll().subscribe(items => this.scales = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.scales?.slice(startIndex,endIndex);
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
    return Math.ceil(this.scales?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const universe = this.scales.find(x => x.id === id);

    if(universe){
      this.scaleService.Toggle(id).subscribe(
        () => {
          this.scaleService.GetAll().subscribe(items => this.scales = items);
        }
      );
    }
  }
}
