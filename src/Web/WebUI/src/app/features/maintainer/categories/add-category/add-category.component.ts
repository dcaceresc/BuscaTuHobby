import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoryService } from '../../../../core/services/maintainer/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-category',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './add-category.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddCategoryComponent implements OnInit { 

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);

  public categoryForm! : FormGroup;

  public ngOnInit(): void {
    this.categoryForm = this.formBuilder.group({
      categoryName: ['',Validators.required],
    });

  }

  public onSubmit() {
    if (this.categoryForm.invalid) {
      return;
    }

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

  public onCancel() {
    this.router.navigate(['/maintainer/categories']);
  }
}
