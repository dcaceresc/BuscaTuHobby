import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoryService } from '../../../../core/services/maintainer/category.service';
import { GroupService } from '../../../../core/services/maintainer/group.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { GroupDto } from '../../../../core/models/maintainer/group.model';

@Component({
  selector: 'app-update-category',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-category.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateCategoryComponent implements OnInit {

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private groupService = inject(GroupService);
  private notificationService = inject(NotificationService);


  public categoryId!: string | null;
  public categoryForm!: FormGroup;
  public groups = signal<GroupDto[]>([]);

  public ngOnInit(): void {
    this.categoryId = this.route.snapshot.paramMap.get('id');
    this.categoryForm = this.formBuilder.group({
      categoryId: [this.categoryId],
      categoryName: ['',Validators.required],
      groupId: ['',Validators.required]
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

    this.loadGroups();

  }

  public loadGroups() {
    this.groupService.getGroups().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.groups.set(response.data);
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
