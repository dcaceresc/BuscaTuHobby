import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../../../../core/services/security/user.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../../../../core/services/notification.service';
import { RoleService } from '../../../../../core/services/security/role.service';
import { RoleDto } from '../../../../../core/models/security/role.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-user',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './add-user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddUserComponent implements OnInit {

  private router = inject(Router);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);
  private roleService = inject(RoleService);

  public userForm! : FormGroup;
  public roles = signal<RoleDto[]>([]);

  public ngOnInit() {
    this.userForm = this.formBuilder.group({
      email: ['',Validators.required],
      roleIds: ['',Validators.required],
    });

    this.loadRoles();
  }

  public loadRoles(){
    this.roleService.getRoles().subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.roles.set(response.data.filter(x => x.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(){
    if(this.userForm.invalid){
      return;
    }

    this.userService.createUser(this.userForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error",response.message);
          return;
        }

        this.notificationService.showSuccess("Success",response.message);
        this.router.navigate(['/security/users']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(){
    this.router.navigate(['/security/users']);
  }

}
