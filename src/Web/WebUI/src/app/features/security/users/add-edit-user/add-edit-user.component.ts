import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RoleDto } from '@app/core/models';
import { NotificationService, RoleService, UserService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-edit-user',
  imports: [ReactiveFormsModule, NgSelectModule, NgClass],
  templateUrl: './add-edit-user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditUserComponent {
  private router = inject(Router);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);
  private roleService = inject(RoleService);

  readonly userId = input.required<string | null>({ alias: "id" });
  public userForm! : FormGroup;
  public roles = signal<RoleDto[]>([]);
  public isEditMode : boolean = false;

  public ngOnInit() {
    this.isEditMode = !!this.userId();
    this.createForm();
    this.getRoles();

    if(this.isEditMode){
      this.userService.getUserById(this.userId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.userForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public createForm() : void {
    if(this.isEditMode){
      this.userForm = this.formBuilder.group({
        userId: [this.userId(), Validators.required],
        email: ['', Validators.required],
        roleIds: ['', Validators.required],
      });
    }else{
      this.userForm = this.formBuilder.group({
        email: ['', Validators.required],
        roleIds: ['', Validators.required],
      });
    }
  }

  public getRoles() {
    this.roleService.getRoles().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.roles.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public onSubmit() {
    if (this.userForm.invalid) {
      return;
    }

    if(this.isEditMode){
      this.userService.updateUser(this.userId(),this.userForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Success', response.message);
          this.router.navigate(['/security/users']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }else{
      this.userService.createUser(this.userForm.value).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.notificationService.showSuccess('Success', response.message);
          this.router.navigate(['/security/users']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        },
      });
    }
  }

  public onCancel() {
    this.router.navigate(['/security/users']);
  }
}
