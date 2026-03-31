import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, input, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PostTypeDto } from '@app/core/models';
import { NotificationService, PostService, PostTypeService, CategoryService } from '@app/core/services';
import { CategoryDto } from '@app/core/models';
import { CustomInputComponent } from '@app/shared/components/custom-input/custom-input.component';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-edit-post',
  imports: [ReactiveFormsModule, CustomInputComponent, NgClass, NgSelectModule],
  templateUrl: './add-edit-post.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditPostComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private postService = inject(PostService);
  private postTypeService = inject(PostTypeService);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);

  readonly postId = input.required<string | null>({ alias: 'id' });
  public isEditMode: boolean = false;
  public postForm!: FormGroup;
  public postTypes = signal<PostTypeDto[]>([]);
  public categories = signal<CategoryDto[]>([]);

  public ngOnInit(): void {
    this.isEditMode = !!this.postId();
    this.createForm();
    this.loadPostTypes();
    this.loadCategories();

    if (this.isEditMode) {
      this.postService.getPostById(this.postId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.postForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public createForm(): void {
    if (this.isEditMode) {
      this.postForm = this.formBuilder.group({
        postId: [this.postId(), Validators.required],
        postTitle: ['', Validators.required],
        postContent: ['', Validators.required],
        postTypeId: ['', Validators.required],
        categoryIds: [[], Validators.required],
      });
    } else {
      this.postForm = this.formBuilder.group({
        postTitle: ['', Validators.required],
        postContent: ['', Validators.required],
        postTypeId: ['', Validators.required],
        categoryIds: [[], Validators.required],
      });
    }
  }

  public loadPostTypes(): void {
    this.postTypeService.getPostTypes().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.postTypes.set(response.data.filter(pt => pt.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.categories.set(response.data.filter(c => c.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public onSubmit() {
    if (this.postForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if (this.isEditMode) {
      this.postService.updatePost(this.postId(), this.postForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Exito', 'Post actualizado correctamente');
          this.router.navigate(['/maintainer/posts']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    } else {
      this.postService.createPost(this.postForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Exito', 'Post creado correctamente');
          this.router.navigate(['/maintainer/posts']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public onCancel() {
    this.router.navigate(['/maintainer/posts']);
  }
}
