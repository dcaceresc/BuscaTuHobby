import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InventoriesService } from 'src/app/core/services/inventories.service';
import { productDto } from 'src/app/core/models/product.model';
import { storeDto } from 'src/app/core/models/store.model';
import { ProductsService } from 'src/app/core/services/products.service';
import { StoresService } from 'src/app/core/services/stores.service';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-inventory.component.html',
  styleUrls: ['./add-inventory.component.scss']
})
export class AddInventoryComponent {
  inventoryForm! : FormGroup;
  products = signal<productDto[]>([]);
  stores = signal<storeDto[]>([]);

  constructor(
    private formbuilder: FormBuilder, 
    private inventoriesService: InventoriesService,
    private productsService : ProductsService,
    private storesService : StoresService, 
    private router:Router) {
    this.createForm();
    this.loadProducts();
    this.loadStores();
  }

  createForm() {
    this.inventoryForm = this.formbuilder.group({
      productId: ['',Validators.required],
      storeId: ['',Validators.required],
      price: [0,Validators.required],
    });
  }

  loadProducts(){
    this.productsService.GetAll().subscribe(
      (products) => {
        this.products.set(products.filter(product => product.active));
      }
    )
  }

  loadStores(){
    this.storesService.GetAll().subscribe(
      (stores) => {
        this.stores.set(stores.filter(store => store.active));
      }
    )
  }

  onSubmit():void{
    if(this.inventoryForm.valid){
      this.inventoriesService.Create(this.inventoryForm.value).subscribe(() => {
        this.router.navigate(['maintainer/inventories']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/inventories']);
  }
}
