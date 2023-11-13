import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductsService } from 'src/app/core/services/products.service';
import { Router } from '@angular/router';
import { ScalesService } from 'src/app/core/services/scales.service';
import { scaleDto } from 'src/app/core/models/scale.model';
import { NgSelectModule } from '@ng-select/ng-select';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';
import { manufacturerDto } from 'src/app/core/models/manufacturer.model';
import { serieByFranchiseDto } from 'src/app/core/models/serie.model';
import { SeriesService } from 'src/app/core/services/series.service';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { categoryDto } from 'src/app/core/models/category.model';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { franchiseDto } from 'src/app/core/models/franchise.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent {
  productForm! : FormGroup;
  scales = signal<scaleDto[]>([]);
  manufacturers = signal<manufacturerDto[]>([]);
  franchises = signal<franchiseDto[]>([]);
  series = signal<serieByFranchiseDto[]>([]);
  categories = signal<categoryDto[]>([]);


  constructor(private formbuilder: FormBuilder, 
    private productsService: ProductsService,
    private scalesService : ScalesService,
    private manufacturersService : ManufacturersService,
    private franschisesService : FranchisesService,
    private seriesService : SeriesService, 
    private categoriesService : CategoriesService,
    private router:Router) {
    this.createForm();
    this.loadScales();
    this.loadManufactures();
    this.loadFranchises();
    this.loadCategories();
  }

  createForm() {
    this.productForm = this.formbuilder.group({
      name: ['',Validators.required],
      scaleId: ['',Validators.required],
      manufacturerId: ['',Validators.required],
      franchiseId: ['',Validators.required],
      serieId: [''],
      categories: ['', [Validators.required]],
      hasBase: [false, Validators.required],
      targetAge: ['',Validators.required],
      size :['',Validators.required],
      releaseDate : ['',Validators.required],
      description: ['',Validators.required],
    });
  }

  loadScales(){
    this.scalesService.GetAll().subscribe(
      (scales) => {
        this.scales.set(scales.filter(scale => scale.active));
      }
    );
  }


  loadManufactures(){
    this.manufacturersService.GetAll().subscribe(
      (manufacturers) => {
        this.manufacturers.set(manufacturers.filter(manufacturer => manufacturer.active));
      }
    )
  }

  loadFranchises(){
    this.franschisesService.GetAll().subscribe(
      (franchises) => {
        this.franchises.set(franchises.filter(franchise => franchise.active));
      }
    ) 
  }


  loadCategories(){
    this.categoriesService.GetAll().subscribe(
      (categories) => {
        this.categories.set(categories.filter(category => category.active));
      }
    )
  }

  onFranchisesChange(){
    const franchiseId = this.productForm.get('franchiseId')!.value;

    if(franchiseId != null){
      this.seriesService.GetbyFranchise(franchiseId).subscribe(
        (series) => {
          this.series.set(series);
        }
      )
    }else{
      this.series.set([]);
    }
    
  }


  searchCategories(term :string, item :any):boolean {
    term = term.toLowerCase();
    return (
      item.groupName.toLowerCase().includes(term) ||
      item.name.toLowerCase().includes(term)
    );
  }


  onSubmit() {
    if (this.productForm.valid) {
      this.productsService.Create(this.productForm.value).subscribe(() => {
        this.router.navigate(['maintainer/products']);
      }, error => {
        // Handle the error
      });
    }
  }

  onCancel() {
    this.router.navigate(['/maintainer/products']);
  }

  
}

