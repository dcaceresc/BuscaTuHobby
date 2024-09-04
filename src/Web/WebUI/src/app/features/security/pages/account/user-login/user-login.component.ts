import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthorizeService } from '../../../../../core/services/security/authorize.service';
import { NotificationService } from '../../../../../core/services/notification.service';

@Component({
  selector: 'app-user-login',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,RouterLink
  ],
  templateUrl: './user-login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserLoginComponent implements OnInit { 

  private authorizeService = inject(AuthorizeService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);
  private router = inject(Router);


  public loginForm!: FormGroup;

  public ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      email: ['',Validators.required],
      password: ['',Validators.required]
    });
  }


  public onSubmit(): void {

    if (this.loginForm.invalid) {
      this.notificationService.showError('Error', 'Favor de llenar los campos');
      return;
    }

    this.authorizeService.login(this.loginForm.value).subscribe({
      next: (response) => {

        if (!response.success){
          this.notificationService.showError('Error', response.message );
        }

        this.router.navigate(['/']);
      },
      error: (error) => {
        this.notificationService.showError('Error', error.message);
        // this.notificationService.showDefaultError();
      }
    });

  }
}
