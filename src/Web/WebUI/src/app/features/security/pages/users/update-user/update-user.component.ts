import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { NotificationService, RoleService, UserService } from '@app/core/services';
import { RoleDto } from '@app/core/models';

@Component({
  selector: 'app-update-user',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './update-user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateUserComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);
  private roleService = inject(RoleService);

  public userForm! : FormGroup;
  public userId! : string | null;
  public roles = signal<RoleDto[]>([]);


  public ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');

    this.userForm = this.formBuilder.group({
      userId: [this.userId],
      email: ['',Validators.required],
      roleIds:['',Validators.required],
      emailConfirmed: [],
      lockoutEnabled: [],
      lockoutEnd: [],
    });

    
    this.loadRoles();
    this.userService.getUserById(this.userId).subscribe({
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

  public loadRoles() {
    this.roleService.getRoles().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.roles.set(response.data.filter(x => x.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }



  public onSubmit(){
    if(this.userForm.invalid){
      return;
    }

    this.userService.updateUser(this.userForm.value).subscribe({
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
