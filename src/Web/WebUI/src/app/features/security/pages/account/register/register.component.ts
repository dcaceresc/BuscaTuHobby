import { CommonModule, NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthorizeService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    RouterLink,ReactiveFormsModule,NgClass
  ],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent { 

  private formBuilder = inject(FormBuilder);
  private authorizeService = inject(AuthorizeService);
  private notificationService = inject(NotificationService);


  public registerForm!: FormGroup;


  public ngOnInit(): void {
    this.registerForm = this.formBuilder.group({
      email: ['',Validators.required],
      password: ['',Validators.required],
      confirmPassword: ['',Validators.required],
      acceptTerms: [false,Validators.requiredTrue]
    });
  }

  public onSubmit(): void {
    if (this.registerForm.invalid) {
      this.notificationService.showError('Error','Por favor complete los datos correctamente');
      return;
    }

    this.authorizeService.register(this.registerForm.value).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.showSuccess('Exito',response.message);
        } else {
          this.notificationService.showError('Error',response.message);
        }
      },
      error: () => {
        this.notificationService.showError('Error','Error al registrar');
      }
    });


  }
}
