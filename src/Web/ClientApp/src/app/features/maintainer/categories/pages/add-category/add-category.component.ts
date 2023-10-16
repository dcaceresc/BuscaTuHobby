import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoriesService } from 'src/app/core/services/categories.service';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-category.component.html',
  styleUrls: ['./add-category.component.scss']
})
export class AddCategoryComponent {
  categoryForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private categoriesService: CategoriesService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.categoryForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.categoryForm.valid){
      this.categoriesService.Create(this.categoryForm.value).subscribe(() => {
        this.router.navigate(['maintainer/categories']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/categories']);
  }
}