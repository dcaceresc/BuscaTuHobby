import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { categoryDto } from 'src/app/core/models/category.model';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { faEdit, faList, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-categories.component.html',
  styleUrls: ['./list-categories.component.scss']
})
export class ListCategoriesComponent {
  categories!:categoryDto[];
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  faList = faList;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private categoriesService:CategoriesService) {}

  ngOnInit(): void {
    this.categoriesService.GetAll().subscribe(items => this.categories = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.categories?.slice(startIndex,endIndex);
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
    return Math.ceil(this.categories?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const category = this.categories.find(x => x.id === id);

    if(category){
      this.categoriesService.Toggle(id).subscribe(
        () => {
          this.categoriesService.GetAll().subscribe(items => this.categories = items);
        }
      );
    }
  }
}
