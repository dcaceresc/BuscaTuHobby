import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../../core/services/maintainer/product.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ScaleService } from '../../../../core/services/maintainer/scale.service';
import { ManufacturerService } from '../../../../core/services/maintainer/manufacturer.service';
import { FranchiseService } from '../../../../core/services/maintainer/franchise.service';
import { SerieService } from '../../../../core/services/maintainer/serie.service';
import { CategoryService } from '../../../../core/services/maintainer/category.service';
import { ScaleDto } from '../../../../core/models/maintainer/scale.model';
import { ManufacturerDto } from '../../../../core/models/maintainer/manufacturer.model';
import { FranchiseDto } from '../../../../core/models/maintainer/franchise.model';
import { SerieByFranchiseDto } from '../../../../core/models/maintainer/serie.model';
import { CategoryDto } from '../../../../core/models/maintainer/category.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-update-product',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './update-product.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateProductComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private productService = inject(ProductService);
  private notificationService = inject(NotificationService);
  private scaleService = inject(ScaleService);
  private manufacturerService = inject(ManufacturerService);
  private franchiseService = inject(FranchiseService);
  private serieService = inject(SerieService);
  private categoryService = inject(CategoryService);


  public productId! :string | null;
  public productForm!: FormGroup;
  public scales = signal<ScaleDto[]>([]);
  public manufacturers = signal<ManufacturerDto[]>([]);
  public franchises = signal<FranchiseDto[]>([]);
  public series = signal<SerieByFranchiseDto[]>([]);
  public categories = signal<CategoryDto[]>([]);
  public today = new Date().toISOString().split('T')[0];


  public ngOnInit(): void {

    this.productId = this.route.snapshot.paramMap.get('id');

    this.productForm = this.formBuilder.group({
      productName: ['',Validators.required],
      scaleId: ['',Validators.required],
      manufacturerId: ['',Validators.required],
      franchiseId: ['',Validators.required],
      serieId: [''],
      productHasBase: [false,Validators.required],
      productTargetAge: [''],
      productSize: ['',Validators.required],
      productDescription: ['',Validators.required],
      productReleaseDate: [this.today,Validators.required],
      categoryIds: ['', Validators.required],
    });

    this.productService.getProductById(this.productId).subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.productForm.patchValue(response.data);

        if (response.data.serieId === null) {
          this.productForm.get('serieId')?.setValue('');
        }

        this.loadSeries();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });

    this.loadScales();
    this.loadManufacturers();
    this.loadFranchises();
    
    this.loadCategories();

  }

  public loadScales(): void {
    this.scaleService.getScales().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.scales.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public loadManufacturers(): void {
    this.manufacturerService.getManufacturers().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.manufacturers.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public loadFranchises(): void {
    this.franchiseService.getFranchises().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.franchises.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public loadSeries(): void {

    var franchiseId = this.productForm.get('franchiseId')?.value;


   if (franchiseId && franchiseId.length !== 0) {
     this.serieService.getSeriesByFranchiseId(franchiseId).subscribe({
       next: (response) => {
         if (!response.success) {
           this.notificationService.showError("Error", response.message);
           return;
         }

            // Clonar la respuesta para evitar modificar el original, si es necesario
           let series = [...response.data];

           // Agregar la nueva columna con el objeto "Todas"
           series.unshift({ serieId: '', serieName: 'Toda la franquicia' });

           // Asignar el array actualizado a `this.series`
           this.series.set(series);

       },
       error: () => {
         this.notificationService.showDefaultError();
       },
     });
   }

   
 }


  public loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.categories.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public onChangeFranchise(): void {
      
    }

  public onSubmit(): void {

  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/products']);
  }

}
