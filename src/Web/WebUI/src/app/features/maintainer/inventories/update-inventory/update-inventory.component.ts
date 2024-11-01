import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductDto, StoreDto } from '@app/core/models';
import { InventoryService, NotificationService, ProductService, StoreService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-update-inventory',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './update-inventory.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateInventoryComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);

  public inventoryId! : string | null;
  public inventoryForm!: FormGroup;
  public products = signal<ProductDto[]>([]);
  public stores = signal<StoreDto[]>([]);


  public ngOnInit(): void {

    this.inventoryId = this.route.snapshot.paramMap.get('id');

    this.inventoryForm = this.formBuilder.group({
      inventoryId: [this.inventoryId, Validators.required],
      storeId: ['', Validators.required],
      productId: ['',Validators.required],
      price: ['', Validators.required]
    });

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

  public onSubmit(){
    if(this.inventoryForm.invalid){
      this.notificationService.showError("Error", "Formulario invalido");
      return;
    }

    this.inventoryService.updateInventory(this.inventoryId,this.inventoryForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/inventories']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(){
    this.router.navigate(['/maintainer/inventories']);
  }

}
