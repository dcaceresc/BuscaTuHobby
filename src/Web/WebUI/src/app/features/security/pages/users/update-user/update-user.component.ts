import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../../../core/services/security/user.service';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NotificationService } from '../../../../../core/services/notification.service';
import { RoleService } from '../../../../../core/services/security/role.service';
import { RoleDto } from '../../../../../core/models/security/role.model';
import { NgSelectModule } from '@ng-select/ng-select';

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
      email: [''],
      roleIds:[''],
      emailConfirmed: [false],
      lockoutEnabled: [false],
      lockoutEnd: [null],
    });

    
    this.loadRoles();
    this.userService.getUserById(this.userId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        console.log(response.data);

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

  }

  public onCancel(){
    this.router.navigate(['/security/users']);
  }

}
