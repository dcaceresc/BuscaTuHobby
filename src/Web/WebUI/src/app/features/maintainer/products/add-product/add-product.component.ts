import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { ProductService } from '../../../../core/services/maintainer/product.service';
import { ScaleService } from '../../../../core/services/maintainer/scale.service';
import { ManufacturerService } from '../../../../core/services/maintainer/manufacturer.service';
import { FranchiseService } from '../../../../core/services/maintainer/franchise.service';
import { SerieService } from '../../../../core/services/maintainer/serie.service';
import { ScaleDto } from '../../../../core/models/maintainer/scale.model';
import { ManufacturerDto } from '../../../../core/models/maintainer/manufacturer.model';
import { FranchiseDto } from '../../../../core/models/maintainer/franchise.model';
import { SerieByFranchiseDto } from '../../../../core/models/maintainer/serie.model';
import { NgSelectModule } from '@ng-select/ng-select';
import { CategoryDto } from '../../../../core/models/maintainer/category.model';
import { CategoryService } from '../../../../core/services/maintainer/category.service';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './add-product.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddProductComponent implements OnInit {
  
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private productService = inject(ProductService);
  private notificationService = inject(NotificationService);
  private scaleService = inject(ScaleService);
  private manufacturerService = inject(ManufacturerService);
  private franchiseService = inject(FranchiseService);
  private serieService = inject(SerieService);
  private categoryService = inject(CategoryService);

  public productForm!: FormGroup;
  public scales = signal<ScaleDto[]>([]);
  public manufacturers = signal<ManufacturerDto[]>([]);
  public franchises = signal<FranchiseDto[]>([]);
  public series = signal<SerieByFranchiseDto[]>([]);
  public categories = signal<CategoryDto[]>([]);
  public today = new Date().toISOString().split('T')[0];
  public productImagePreview = signal<string[]>([]);
  public productImage = signal<File[]>([]);


  public ngOnInit(): void {
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

    this.loadScales();
    this.loadManufacturers();
    this.loadFranchises();
    this.loadSeries();
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

    this.productForm.get('serieId')?.setValue(null);

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
            series.unshift({ serieId: null, serieName: 'Toda la franquicia' });

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
    this.loadSeries();
  }

  public onImageChange(event: any): void {
    const files = event.target.files;
    const readers = [];
    const maxSize = 5 * 1024 * 1024; // Máximo de 5MB

    this.productImagePreview.set([]);
  
    this.productImage.set(files);
  
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
  
      if (!file.type.startsWith('image/')) {
        this.notificationService.showError('Error', 'Solo se permiten imágenes');
        continue;
      }
  
      if (file.size > maxSize) {
        this.notificationService.showError('Error', 'El archivo supera el tamaño máximo permitido (5MB)');
        continue;
      }
  
      const reader = new FileReader();
      readers.push(reader);
  
      reader.onload = (e) => {
        const base64Image = e.target!.result as string;
        this.productImagePreview.update(images => [...images, base64Image]);
      };
  
      reader.readAsDataURL(file);
    }
  }


  public onSubmit(): void {

    if (this.productForm.invalid) {
      return;
    }

    if (this.productImage().length === 0) {
      this.notificationService.showError('Error', 'Debe seleccionar al menos una imagen');
      return;
    }


    this.productService.createProduct(this.productForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }
        

        const productId = response.data as string;

        const formData = new FormData();

        for(let i = 0; i < this.productImage().length; i++){
          formData.append('productImages', this.productImage()[i]);
        }

        this.productService.createProductImages(productId, formData).subscribe({
          next: (response) => {
            if(!response.success){
              this.notificationService.showError("Error",response.message);
              return;
            }
          },
          error: () => {
            this.notificationService.showDefaultError();
          },
        });



        this.router.navigate(['/maintainer/products']);
      },
      error: (error) => {
        console.log(error);
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/products']);
  }

}
