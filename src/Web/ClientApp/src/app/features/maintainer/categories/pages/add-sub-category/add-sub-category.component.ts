import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-sub-category.component.html',
  styleUrls: ['./add-sub-category.component.scss']
})
export class AddSubCategoryComponent {
  subCategoryForm! : FormGroup;
  categoryId!: string;

  constructor(private formbuilder: FormBuilder, private categoriesService: CategoriesService, private router:Router, private route:ActivatedRoute) {
    this.categoryId = this.route.snapshot.paramMap.get('id')!
    this.createForm();
  }

  createForm() {
    this.subCategoryForm = this.formbuilder.group({
      name: ['',Validators.required],
      categoryId: [this.categoryId,Validators.required]
    });
  }

  onSubmit():void{
    if(this.subCategoryForm.valid){
      this.categoriesService.CreateSubCategory(this.subCategoryForm.value).subscribe(() => {
        this.router.navigate([`maintainer/categories/${this.categoryId}/subcategories`]);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate([`maintainer/categories/${this.categoryId}/subcategories`]);
  }
}