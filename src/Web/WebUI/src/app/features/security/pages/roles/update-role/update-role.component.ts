import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationService, RoleService } from '@app/core/services';

@Component({
  selector: 'app-update-role',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-role.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateRoleComponent implements OnInit { 

  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private roleService = inject(RoleService);
  private notificationService = inject(NotificationService);


  public roleForm!: FormGroup;
  public roleId!: string | null;

  public ngOnInit(): void {
    this.roleId = this.route.snapshot.paramMap.get('id');
    this.roleForm = this.formBuilder.group({
      roleId: [this.roleId],
      roleName: ['',Validators.required],
    });

    this.loadRole();
  }

  public loadRole() {
    this.roleService.getRoleById(this.roleId).subscribe({
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

  public onSubmit(){
    if (this.roleForm.invalid) {
      return;
    }

    this.roleService.updateRole(this.roleId,this.roleForm.value).subscribe({
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

  public onCancel() {
    this.router.navigate(['/security/roles']);
  }
}
