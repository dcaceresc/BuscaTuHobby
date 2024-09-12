import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoleService } from '../../../../../core/services/security/role.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-add-role',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-role.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddRoleComponent implements OnInit {

  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private roleService = inject(RoleService);
  private notificationService = inject(NotificationService);

  public roleForm!: FormGroup;

  public ngOnInit(): void {
    this.roleForm = this.formBuilder.group({
      roleName: ['', Validators.required],
    });
  }

  public onSubmit() {
    if (this.roleForm.invalid) {
      return;
    }

    this.roleService.addRole(this.roleForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.notificationService.showSuccess('Exito', response.message);
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
