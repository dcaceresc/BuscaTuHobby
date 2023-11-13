import { Component, Signal, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductsService } from 'src/app/core/services/products.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ScalesService } from 'src/app/core/services/scales.service';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { SeriesService } from 'src/app/core/services/series.service';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { scaleDto } from 'src/app/core/models/scale.model';
import { manufacturerDto } from 'src/app/core/models/manufacturer.model';
import { franchiseDto } from 'src/app/core/models/franchise.model';
import { serieByFranchiseDto } from 'src/app/core/models/serie.model';
import { categoryDto } from 'src/app/core/models/category.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  providers:[DatePipe],
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './update-product.component.html',
  styleUrls: ['./update-product.component.scss']
})
export class UpdateProductComponent {
  productId!:string | null;
  productForm! : FormGroup;
  scales = signal<scaleDto[]>([]);
  manufacturers = signal<manufacturerDto[]>([]);
  franchises = signal<franchiseDto[]>([]);
  series = signal<serieByFranchiseDto[]>([]);
  categories = signal<categoryDto[]>([]);

  constructor(private productsService: ProductsService,
    private scalesService : ScalesService,
    private manufacturersService : ManufacturersService,
    private franschisesService : FranchisesService,
    private seriesService : SeriesService, 
    private categoriesService : CategoriesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private datePipe:DatePipe) {
      this.productId = this.route.snapshot.paramMap.get('id');
      this.loadCategories();
      this.loadFranchises();
      this.loadManufactures();
      this.loadScales();
  }

  ngOnInit(): void {
    this.productForm = this.formBuilder.group({
      id:[this.productId,Validators.required],
      name: ['', Validators.required],
      scaleId: ['', Validators.required],
      manufacturerId: ['', Validators.required],
      franchiseId: ['', Validators.required],
      serieId: [''],
      categories : ['',Validators.required],
      hasBase: ['', Validators.required],
      size: ['', Validators.required],
      targetAge: ['', Validators.required],
      releaseDate: ['', Validators.required],
      description: ['', Validators.required]
    });

    this.productsService.GetbyId(this.productId).subscribe(
      (product) => {
        // Llena el formulario con los datos del ambiente
        this.productForm.patchValue({
          name: product.name,
          scaleId: product.scaleId,
          manufacturerId: product.manufacturerId,
          franchiseId: product.franchiseId,
          serieId: product.serieId,
          hasBase: product.hasBase,
          size: product.size,
          targetAge:product.targetAge,
          releaseDate: this.datePipe.transform(product.releaseDate,'yyyy-MM-dd'),
          description:product.description,
          categories:product.categories

          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
        this.loadSeries();
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );

    
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

  loadSeries(){
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


  onFranchisesChange(){
    this.loadSeries();
  }

  searchCategories(term :string, item :any):boolean {
    term = term.toLowerCase();
    return (
      item.groupName.toLowerCase().includes(term) ||
      item.name.toLowerCase().includes(term)
    );
  }

  onSubmit(): void {
    if (this.productForm.valid) {
      this.productsService.Update(this.productId,this.productForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/products']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/products']);
  }
}