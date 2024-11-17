import { ChangeDetectionStrategy, Component, inject, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MenuService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-add-edit-menu',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditMenuComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private menuService = inject(MenuService);
  private notificationService = inject(NotificationService);

  @Input('id') menuId!: string | null;
  public isEditMode: boolean = false;
  public menuForm!: FormGroup;

  public ngOnInit(): void {
    this.isEditMode = !!this.menuId;
    this.createForm();

    if(this.isEditMode){
      this.menuService.getMenuById(this.menuId).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }

          this.menuForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.menuForm = this.formBuilder.group({
        menuId: [this.menuId, Validators.required],
        menuName: ['', Validators.required],
        menuOrder: ['', Validators.required],
      });
    }else{
      this.menuForm = this.formBuilder.group({
        menuName: ['', Validators.required],
        menuOrder: ['', Validators.required],
      });
    }
  }

  public onSubmit(): void {
    if (this.menuForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.menuService.updateMenu(this.menuId,this.menuForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }

          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/menus']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }else{
      this.menuService.createMenu(this.menuForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/menus']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/menus']);
  }
}
