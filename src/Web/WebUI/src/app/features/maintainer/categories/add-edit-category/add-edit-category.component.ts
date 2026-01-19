import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoryService, NotificationService } from '@app/core/services';
import { CustomInputComponent } from '@app/shared/components/custom-input/custom-input.component';

@Component({
    selector: 'app-add-edit-category',
    imports: [ReactiveFormsModule, CustomInputComponent, NgClass],
    templateUrl: './add-edit-category.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditCategoryComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);

  readonly categoryId = input.required<string | null>({ alias: "id" });
  public isEditMode : boolean = false;
  public categoryForm! : FormGroup;

  public ngOnInit(): void {
    this.isEditMode = !!this.categoryId();
    this.createForm();

    if(this.isEditMode){
      this.categoryService.getCategoryById(this.categoryId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.categoryForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }

  }

  public createForm() : void {

    if(this.isEditMode){
      this.categoryForm = this.formBuilder.group({
        categoryId: [this.categoryId(), Validators.required],
        categoryName: ['', Validators.required],
        categoryIcon: ['', Validators.required],
        categoryOrder: ['', Validators.required],
        categorySlug: ['', Validators.required],
      });
    }else{
      this.categoryForm = this.formBuilder.group({
        categoryName: ['', Validators.required],
        categoryIcon: ['', Validators.required],
        categoryOrder: ['', Validators.required],
        categorySlug: ['', Validators.required],
      });
    }

  }

  public onSubmit() {
    if (this.categoryForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.categoryService.updateCategory(this.categoryId(),this.categoryForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Éxito', 'Categoría actualizada correctamente');
          this.router.navigate(['/maintainer/categories']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }else{
      this.categoryService.createCategory(this.categoryForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Éxito', 'Categoría creada correctamente');
          this.router.navigate(['/maintainer/categories']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public onCancel() {
    this.router.navigate(['/maintainer/categories']);
  }
}
