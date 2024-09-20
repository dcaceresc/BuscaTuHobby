import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfigurationService } from '../../../../core/services/maintainer/configuration.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-update-configuration',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-configuration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateConfigurationComponent implements OnInit { 
  
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private configurationService = inject(ConfigurationService);
  private notificationService = inject(NotificationService);
  private formBuilder = inject(FormBuilder);

  public configurationId!: string | null;
  public configurationForm!: FormGroup;


  public ngOnInit(): void {
    this.configurationId = this.route.snapshot.paramMap.get('id');
    this.configurationForm = this.formBuilder.group({
      configurationId : [this.configurationId],
      configurationName: ['',Validators.required],
      configurationValue: ['',Validators.required],
    });

    this.configurationService.getConfigurationById(this.configurationId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.configurationForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });

    
  }

  public onSubmit(): void{
    if(this.configurationForm.invalid){
      return;
    }

    this.configurationService.updateConfiguration(this.configurationId,this.configurationForm.value).subscribe({
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
