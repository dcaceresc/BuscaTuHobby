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
import { serieByFranchiseDto, serieDto } from 'src/app/core/models/serie.model';
import { SeriesService } from 'src/app/core/services/series.service';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { categoryDto } from 'src/app/core/models/category.model';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { franchiseDto } from 'src/app/core/models/franchise.model';
import { GroupsService } from 'src/app/core/services/groups.service';
import { groupDto } from 'src/app/core/models/group.model';

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
  franchises:franchiseDto[] =[];
  series:serieByFranchiseDto[] = [];
  categories : categoryDto[] = [];
  selectedCategoryGroup:string[] = [];


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

  loadFranchises(){
    this.franschisesService.GetAll().subscribe(
      (franchises) => {
        this.franchises = franchises.filter(franchise => franchise.active);
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

  onFranchisesChange(){
    const franchiseId = this.productForm.get('franchiseId')!.value;

    if(franchiseId != null){
      this.seriesService.GetbyFranchise(franchiseId).subscribe(
        (series) => {
          this.series = series;
        }
      )
    }else{
      this.series = [];
    }
    
  }


  // onCategoriesChange(){
  //   const category = this.categories.filter(category => category.id this.productForm.get('categories')?.value);

  //   console.log(category);

  //   if(!category){
  //     this.selectedCategoryGroup = [];
  //     this.loadCategories();
  //   }else{
  //     // this.selectedCategoryGroup.push(category?.groupName!);
  //   }

  //   console.log(this.selectedCategoryGroup);
  //   this.categories = this.categories.filter(category => !this.selectedCategoryGroup.includes(category.groupName));

    
  // }

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

