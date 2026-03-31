import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService, PostTypeService } from '@app/core/services';
import { CustomInputComponent } from '@app/shared/components/custom-input/custom-input.component';

@Component({
  selector: 'app-add-edit-post-type',
  imports: [ReactiveFormsModule, CustomInputComponent, NgClass],
  templateUrl: './add-edit-post-type.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditPostTypeComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private postTypeService = inject(PostTypeService);
  private notificationService = inject(NotificationService);

  readonly postTypeId = input.required<string | null>({ alias: 'id' });
  public isEditMode: boolean = false;
  public postTypeForm!: FormGroup;

  public ngOnInit(): void {
    this.isEditMode = !!this.postTypeId();
    this.createForm();

    if (this.isEditMode) {
      this.postTypeService.getPostTypeById(this.postTypeId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.postTypeForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public createForm(): void {
    if (this.isEditMode) {
      this.postTypeForm = this.formBuilder.group({
        postTypeId: [this.postTypeId(), Validators.required],
        postTypeName: ['', Validators.required],
      });
    } else {
      this.postTypeForm = this.formBuilder.group({
        postTypeName: ['', Validators.required],
      });
    }
  }

  public onSubmit() {
    if (this.postTypeForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if (this.isEditMode) {
      this.postTypeService.updatePostType(this.postTypeId(), this.postTypeForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Exito', 'Tipo de post actualizado correctamente');
          this.router.navigate(['/maintainer/post-types']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    } else {
      this.postTypeService.createPostType(this.postTypeForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Exito', 'Tipo de post creado correctamente');
          this.router.navigate(['/maintainer/post-types']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public onCancel() {
    this.router.navigate(['/maintainer/post-types']);
  }
}
