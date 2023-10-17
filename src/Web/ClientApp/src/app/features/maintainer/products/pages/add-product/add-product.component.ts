import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductsService } from 'src/app/core/services/products.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.scss']
})
export class AddProductComponent {
  productForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private productsService: ProductsService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.productForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.productForm.valid){
      this.productsService.Create(this.productForm.value).subscribe(() => {
        this.router.navigate(['maintainer/products']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/products']);
  }
}
