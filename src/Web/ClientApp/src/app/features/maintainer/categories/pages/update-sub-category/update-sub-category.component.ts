import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { subCategoryVM } from 'src/app/core/models/category.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-sub-category.component.html',
  styleUrls: ['./update-sub-category.component.scss']
})
export class UpdateSubCategoryComponent {
  categoryId!:string | null;
  subCategoryId!:string | null;
  subCategory!: subCategoryVM;
  subCategoryForm! : FormGroup;

  constructor(private categoriesService: CategoriesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.categoryId = this.route.snapshot.paramMap.get('id');
      this.subCategoryId = this.route.snapshot.paramMap.get('subcategoryid');
  }

  ngOnInit(): void {
    this.subCategoryForm = this.formBuilder.group({
      id:[this.subCategoryId,Validators.required],
      name: ['', Validators.required],
    });

    this.categoriesService.GetSubCategorybyId(this.subCategoryId,this.categoryId).subscribe(
      (subCategory) => {
        // Llena el formulario con los datos del ambiente
        this.subCategoryForm.patchValue({
          name: subCategory.name,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
  }

  onSubmit(): void {
    if (this.subCategoryForm.valid) {
      this.categoriesService.UpdateSubCategory(this.subCategoryId,this.categoryId,this.subCategoryForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/categories',this.categoryId,'subcategories']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/categories',this.categoryId,'subcategories']);
  }
}