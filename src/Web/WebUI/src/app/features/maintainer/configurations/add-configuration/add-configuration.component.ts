import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfigurationService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-add-configuration',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-configuration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddConfigurationComponent implements OnInit {
  
  private router = inject(Router);
  private configurationService = inject(ConfigurationService);
  private notificationService = inject(NotificationService);
  private formBuilder = inject(FormBuilder);

  public configurationForm!: FormGroup;


  public ngOnInit(): void {
    this.configurationForm = this.formBuilder.group({
      configurationName: ['', Validators.required],
      configurationValue: ['', Validators.required],
    });
  }


  public onSubmit(): void{
    if(this.configurationForm.invalid){
      return;
    }

    this.configurationService.addConfiguration(this.configurationForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.router.navigate(['/maintainer/configurations']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void{
    this.router.navigate(['/maintainer/configurations']);
  }

}
