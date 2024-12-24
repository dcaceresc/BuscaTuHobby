import { ChangeDetectionStrategy, Component, inject, input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService, RoleService } from '@app/core/services';

@Component({
  selector: 'app-add-edit-role',
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-role.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditRoleComponent implements OnInit {

  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private roleService = inject(RoleService);
  private notificationService = inject(NotificationService);

  readonly roleId = input.required<string | null>({ alias: "id" });
  public roleForm!: FormGroup;
  public isEditMode : boolean = false;
  

  public ngOnInit() {
    this.isEditMode = !!this.roleId();
    this.createForm();

    if(this.isEditMode){
      this.roleService.getRoleById(this.roleId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.roleForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public createForm() : void {
    if(this.isEditMode){
      this.roleForm = this.formBuilder.group({
        roleId: [this.roleId(), Validators.required],
        roleName: ['', Validators.required],
      });
    }else{
      this.roleForm = this.formBuilder.group({
        roleName: ['', Validators.required],
      });
    }
  }

  public onSubmit() {
    if (this.roleForm.invalid) {
      return;
    }

    if(this.isEditMode){
      this.roleService.updateRole(this.roleId(),this.roleForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Success', response.message);
          this.router.navigate(['/security/roles']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }else{
      this.roleService.addRole(this.roleForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Success', response.message);
          this.router.navigate(['/security/roles']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public onCancel() {
    this.router.navigate(['/security/roles']);
  }

}
