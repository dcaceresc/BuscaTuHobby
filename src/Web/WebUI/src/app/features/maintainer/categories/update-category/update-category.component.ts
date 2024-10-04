import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoryService } from '../../../../core/services/maintainer/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-update-category',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './update-category.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateCategoryComponent implements OnInit {

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);


  public categoryId!: string | null;
  public categoryForm!: FormGroup;

  public ngOnInit(): void {
    this.categoryId = this.route.snapshot.paramMap.get('id');
    this.categoryForm = this.formBuilder.group({
      categoryId: [this.categoryId],
      categoryName: ['',Validators.required],
    });

    this.categoryService.getCategoryById(this.categoryId).subscribe({
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


  public onSubmit() {
    if (this.categoryForm.invalid) {
      return;
    }

    this.categoryService.updateCategory(this.categoryId, this.categoryForm.value).subscribe({
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
  }

  public onCancel() {
    this.router.navigate(['/maintainer/categories']);
  }

}
