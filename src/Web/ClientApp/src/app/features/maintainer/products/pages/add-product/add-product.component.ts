import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductsService } from 'src/app/core/services/products.service';
import { Router } from '@angular/router';
import { ScalesService } from 'src/app/core/services/scales.service';
import { scaleDto } from 'src/app/core/models/scale.model';
import { NgSelectModule } from '@ng-select/ng-select';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';
import { manufacturerDto } from 'src/app/core/models/manufacturer.model';
import { serieDto } from 'src/app/core/models/serie.model';
import { SeriesService } from 'src/app/core/services/series.service';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { categoryDto } from 'src/app/core/models/category.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent {
  productForm! : FormGroup;
  scales:scaleDto[] = [];
  manufacturers:manufacturerDto[] = [];
  series:serieDto[] = [];
  categories : categoryDto[] = [];

  constructor(private formbuilder: FormBuilder, 
    private productsService: ProductsService,
    private scalesService : ScalesService,
    private manufacturersService : ManufacturersService,
    private seriesService : SeriesService, 
    private categoriesService : CategoriesService,
    private router:Router) {
    this.createForm();
    this.loadScales();
    this.loadManufactures();
    this.loadSeries();
    this.loadCategories();
  }

  createForm() {
    this.productForm = this.formbuilder.group({
      name: ['',Validators.required],
      scaleId: ['',Validators.required],
      manufacturerId: ['',Validators.required],
      serieId: ['',Validators.required],
      categoryId : ['',Validators.required],
      hasBase: [false, Validators.required],
      releaseDate : ['',Validators.required],
      description: ['',Validators.required],
    });
  }

  loadScales(){
    this.scalesService.GetAll().subscribe(
      (scales) => {
        this.scales = scales.filter(scale => scale.active);
      }
    );
  }


  loadManufactures(){
    this.manufacturersService.GetAll().subscribe(
      (manufacturers) => {
        this.manufacturers = manufacturers.filter(manufacturer => manufacturer.active);
      }
    )
  }

  loadSeries(){
    this.seriesService.GetAll().subscribe(
      (series) => {
        this.series = series.filter(serie => serie.active); 
      }
    )
  }

  loadCategories(){
    this.categoriesService.GetAll().subscribe(
      (categories) => {
        this.categories = categories;
      }
    )
  }

  searchCategories(term :string, item :any):boolean {
    term = term.toLowerCase();
    return (
      item.groupName.toLowerCase().includes(term) ||
      item.name.toLowerCase().includes(term)
    );
  }

  onSubmit():void{
    if(this.productForm.valid){
      this.productsService.Create(this.productForm.value).subscribe(() => {
        this.router.navigate(['maintainer/products']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/products']);
  }
}
