import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    RouterLink,ReactiveFormsModule
  ],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent { 

  private formBuilder = inject(FormBuilder);


  public registerForm!: FormGroup;


  public ngOnInit(): void {
    this.registerForm = this.formBuilder.group({
      email: [''],
      password: [''],
      confirmPassword: ['']
    });
  }


  public onSubmit(): void {
  }
}
