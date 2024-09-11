import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../../../../core/services/security/user.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NotificationService } from '../../../../../core/services/notification.service';

@Component({
  selector: 'app-add-user',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './add-user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddUserComponent implements OnInit {

  private router = inject(Router);
  private userService = inject(UserService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);

  public userForm! : FormGroup;

  public ngOnInit() {
    this.userForm = this.formBuilder.group({
      email: ['',Validators.required],
      password: ['',Validators.required],
      rolesId: ['',Validators.required],
    });
  }

}
