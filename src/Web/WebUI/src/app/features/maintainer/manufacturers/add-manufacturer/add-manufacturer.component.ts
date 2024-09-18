import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ManufacturerService } from '../../../../core/services/maintainer/manufacturer.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-add-manufacturer',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-manufacturer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddManufacturerComponent implements OnInit { 
  
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private manufacturerService = inject(ManufacturerService);
  private notificationService = inject(NotificationService);

  public manufacturerForm!: FormGroup;

  public ngOnInit(): void {
    this.manufacturerForm = this.formBuilder.group({
      manufacturerName: ['', Validators.required]
    });
  }

  public onSubmit(): void {
    if (this.manufacturerForm.invalid) {
      return;
    }

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

  public onCancel(): void {
    this.router.navigate(['/maintainer/manufacturers']);
  }
}
