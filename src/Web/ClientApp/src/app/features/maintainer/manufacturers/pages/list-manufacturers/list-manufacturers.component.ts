import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';
import { manufacturerDto } from 'src/app/core/models/manufacturer.model';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-manufacturers.component.html',
  styleUrls: ['./list-manufacturers.component.scss']
})
export class ListManufacturersComponent {
  manufacturers = signal<manufacturerDto[]>([]);
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = signal(1);
  itemsPerPage = 10;

  constructor(private manufacturersService:ManufacturersService) {}

  ngOnInit(): void {
    this.manufacturersService.GetAll().subscribe(items => this.manufacturers.set(items));
  }

  getPaginatedData(){
    const startIndex = (this.currentPage() - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.manufacturers()?.slice(startIndex,endIndex);
  }

  setPage(pageNumber: number) {
    this.currentPage.set(pageNumber);
  }

  range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  nPage(){
    return Math.ceil(this.manufacturers()?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const manufacturer = this.manufacturers().find(x => x.id === id);

    if(manufacturer){
      this.manufacturersService.Toggle(id).subscribe(
        () => {
          this.manufacturersService.GetAll().subscribe(items => this.manufacturers.set(items));
        }
      );
    }
  }
}
