import { Component } from '@angular/core';
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
  manufacturerForm! : FormGroup;
  products : productDto[] =[];
  stores : storeDto[] =[];

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
    this.manufacturerForm = this.formbuilder.group({
      productId: ['',Validators.required],
      storeId: ['',Validators.required],
      price: [0,Validators.required],
    });
  }

  loadProducts(){
    this.productsService.GetAll().subscribe(
      (products) => {
        this.products = products;
      }
    )
  }

  loadStores(){
    this.storesService.GetAll().subscribe(
      (stores) => {
        this.stores = stores;
      }
    )
  }

  onSubmit():void{
    if(this.manufacturerForm.valid){
      this.inventoriesService.Create(this.manufacturerForm.value).subscribe(() => {
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
