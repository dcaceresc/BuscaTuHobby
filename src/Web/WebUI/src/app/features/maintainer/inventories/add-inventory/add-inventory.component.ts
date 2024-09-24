import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../../../core/services/maintainer/inventory.service';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { ProductService } from '../../../../core/services/maintainer/product.service';
import { StoreService } from '../../../../core/services/maintainer/store.service';
import { ProductDto } from '../../../../core/models/maintainer/product.model';
import { StoreDto } from '../../../../core/models/maintainer/store.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-inventory',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule, NgSelectModule
  ],
  templateUrl: './add-inventory.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddInventoryComponent implements OnInit {

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);

  public inventoryForm!: FormGroup;
  public products = signal<ProductDto[]>([]);
  public stores = signal<StoreDto[]>([]);


  public ngOnInit(): void {
    this.inventoryForm = this.formBuilder.group({
      storeId: ['', Validators.required],
      productId: ['',Validators.required],
      price: ['', Validators.required]
    });

    this.loadProducts();
    this.loadStores();
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
      return;
    }

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

  public onCancel(): void {
    this.router.navigate(['/maintainer/inventories']);
  }


    
}
