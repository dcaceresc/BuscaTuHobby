import { ChangeDetectionStrategy, Component, inject, OnInit, input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ManufacturerService, NotificationService } from '@app/core/services';

@Component({
    selector: 'app-add-edit-manufacturer',
    imports: [ReactiveFormsModule],
    templateUrl: './add-edit-manufacturer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditManufacturerComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private manufacturerService = inject(ManufacturerService);
  private notificationService = inject(NotificationService);

  readonly manufacturerId = input.required<string | null>({ alias: "id" });
  public isEditMode : boolean = false;
  public manufacturerForm!: FormGroup;

  public ngOnInit(): void {
    this.isEditMode = !!this.manufacturerId();
    this.createForm();

    if(this.isEditMode){
      this.manufacturerService.getManufacturerById(this.manufacturerId()).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }

          this.manufacturerForm.patchValue(response.data);

        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.manufacturerForm = this.formBuilder.group({
        manufacturerId: [this.manufacturerId(), Validators.required],
        manufacturerName: ['', Validators.required]
      });
    }else{
      this.manufacturerForm = this.formBuilder.group({
        manufacturerName: ['', Validators.required]
      });
    }
  }

  public onSubmit(): void {
    if (this.manufacturerForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.manufacturerService.updateManufacturer(this.manufacturerId(), this.manufacturerForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/manufacturers']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }else{
      this.manufacturerService.createManufacturer(this.manufacturerForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/manufacturers']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/manufacturers']);
  }
}
