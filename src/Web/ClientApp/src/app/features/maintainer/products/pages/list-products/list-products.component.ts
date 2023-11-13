import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { productDto } from 'src/app/core/models/product.model';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { ProductsService } from 'src/app/core/services/products.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-products.component.html',
  styleUrls: ['./list-products.component.scss']
})
export class ListProductsComponent {
  products = signal<productDto[]>([]);
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = signal(1);
  itemsPerPage = 10;

  constructor(private productsService: ProductsService) { }

  ngOnInit(): void {
    this.productsService.GetAll().subscribe(items => this.products.set(items));
  }

  getPaginatedData(){
    const startIndex = (this.currentPage() - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.products()?.slice(startIndex,endIndex);
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
    return Math.ceil(this.products()?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const store = this.products().find(x => x.id === id);

    if(store){
      this.productsService.Toggle(id).subscribe(
        () => {
          this.productsService.GetAll().subscribe(items => this.products.set(items));
        }
      );
    }
  }
}
