import { ChangeDetectionStrategy, Component, inject, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfigurationService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-add-edit-configuration',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './add-edit-configuration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditConfigurationComponent implements OnInit {
  private router = inject(Router);
  private configurationService = inject(ConfigurationService);
  private notificationService = inject(NotificationService);
  private formBuilder = inject(FormBuilder);

  @Input('id') configurationId!: string | null;
  public isEditMode: boolean = false;
  public configurationForm!: FormGroup;


  public ngOnInit(): void {
    this.isEditMode = !!this.configurationId;
    this.createForm();

    if(this.isEditMode){
      this.configurationService.getConfigurationById(this.configurationId).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.configurationForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.configurationForm = this.formBuilder.group({
        configurationId: [this.configurationId, Validators.required],
        configurationName: ['', Validators.required],
        configurationValue: ['', Validators.required],
      });
    }else{
      this.configurationForm = this.formBuilder.group({
        configurationName: ['', Validators.required],
        configurationValue: ['', Validators.required],
      });
    }
  }


  public onSubmit(): void{
    if(this.configurationForm.invalid){
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.configurationService.updateConfiguration(this.configurationId,this.configurationForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/configurations']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
      return;
    }else{
      this.configurationService.addConfiguration(this.configurationForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/configurations']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public onCancel(): void{
    this.router.navigate(['/maintainer/configurations']);
  }
}
