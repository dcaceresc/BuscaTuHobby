import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { productDto } from 'src/app/core/models/product.model';
import { storeDto } from 'src/app/core/models/store.model';
import { InventoriesService } from 'src/app/core/services/inventories.service';
import { ProductsService } from 'src/app/core/services/products.service';
import { StoresService } from 'src/app/core/services/stores.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './update-inventory.component.html',
  styleUrls: ['./update-inventory.component.scss']
})
export class UpdateInventoryComponent {
  inventoryId!:string | null;
  inventoryForm! : FormGroup;
  products = signal<productDto[]>([]);
  stores = signal<storeDto[]>([]);



  constructor(
    private formBuilder: FormBuilder, 
    private inventoriesService: InventoriesService,
    private productsService : ProductsService,
    private storesService : StoresService, 
    private router:Router,
    private route: ActivatedRoute) {
      this.inventoryId = this.route.snapshot.paramMap.get('id');
      this.loadProducts();
      this.loadStores();
  }

  ngOnInit(): void {
    this.inventoryForm = this.formBuilder.group({
      id:[this.inventoryId,Validators.required],
      productId: ['', Validators.required],
      storeId: ['', Validators.required],
      price: ['', Validators.required],
    });

    this.inventoriesService.GetbyId(this.inventoryId).subscribe(
      (inventory) => {
        // Llena el formulario con los datos del ambiente
        this.inventoryForm.patchValue({
          productId: inventory.productId,
          storeId: inventory.storeId,
          price: inventory.price,

          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );

    
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

  onCancel():void{
    this.router.navigate(['/maintainer/inventories']);
  }
}
