import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { subCategoryDto } from 'src/app/core/models/category.model';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faList, faPlus, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { CategoriesService } from 'src/app/core/services/categories.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-sub-categories.component.html',
  styleUrls: ['./list-sub-categories.component.scss']
})
export class ListSubCategoriesComponent {
  subCategories!:subCategoryDto[];
  categoryName! : string;
  categoryId! : string;
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  faList = faList;
  faPlus = faPlus;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private categoriesService:CategoriesService,private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.categoryId = this.route.snapshot.paramMap.get('id')!
    this.categoriesService.GetAllSubCategory(this.categoryId).subscribe(items => this.subCategories = items);
    this.categoriesService.GetbyId(this.categoryId).subscribe(item => this.categoryName = item.name);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.subCategories?.slice(startIndex,endIndex);
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
    return Math.ceil(this.subCategories?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const universe = this.subCategories.find(x => x.id === id);

    if(universe){
      this.categoriesService.Toggle(id).subscribe(
        () => {
          this.categoriesService.GetAllSubCategory(this.categoryId).subscribe(items => this.subCategories = items);
        }
      );
    }
  }
}
