import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { storeDto } from 'src/app/core/models/store.model';
import { StoresService } from 'src/app/core/services/stores.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink, FontAwesomeModule],
  templateUrl: './list-stores.component.html',
  styleUrls: ['./list-stores.component.scss']
})
export class ListStoresComponent implements OnInit {
  stores: storeDto[] = []
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;


  constructor(private storesService: StoresService) { }

  ngOnInit(): void {
    this.storesService.GetAll().subscribe(items => this.stores = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.stores?.slice(startIndex,endIndex);
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
    return Math.ceil(this.stores?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const universe = this.stores.find(x => x.id === id);

    if(universe){
      this.storesService.Toggle(id).subscribe(
        () => {
          this.storesService.GetAll().subscribe(items => this.stores = items);
        }
      );
    }
  }
}
