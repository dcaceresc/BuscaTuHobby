import { ChangeDetectionStrategy, Component, inject, Input, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductDto, StoreDto } from '@app/core/models';
import { InventoryService, NotificationService, ProductService, StoreService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-edit-inventory',
  standalone: true,
  imports: [ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-edit-inventory.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditInventoryComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);

  @Input('id') inventoryId!: string | null;
  public isEditMode : boolean = false;
  public inventoryForm!: FormGroup;
  public products = signal<ProductDto[]>([]);
  public stores = signal<StoreDto[]>([]);


  public ngOnInit(): void {
    this.isEditMode = !!this.inventoryId;
    this.createForm();
    this.loadProducts();
    this.loadStores();

    if(this.isEditMode){
      this.inventoryService.getInventoryById(this.inventoryId).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.inventoryForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.inventoryForm = this.formBuilder.group({
        inventoryId : [this.inventoryId, Validators.required],
        storeId: ['', Validators.required],
        productId: ['',Validators.required],
        price: ['', Validators.required]
      });
    }else{
      this.inventoryForm = this.formBuilder.group({
        storeId: ['', Validators.required],
        productId: ['',Validators.required],
        price: ['', Validators.required]
      });
    }
  }

  public loadProducts(){
    this.productService.getProducts().subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }
        this.products.set(response.data.filter(x => x.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public loadStores(){
    this.storeService.getStores().subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }
        this.stores.set(response.data.filter(x => x.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(): void {
    if (this.inventoryForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.inventoryService.updateInventory(this.inventoryId,this.inventoryForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.notificationService.showSuccess("Exito", "Inventario actualizado correctamente");
          this.router.navigate(['/maintainer/inventories']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }else{
      this.inventoryService.createInventory(this.inventoryForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.notificationService.showSuccess("Exito", "Inventario creado correctamente");
          this.router.navigate(['/maintainer/inventories']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }

  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/inventories']);
  }
}
