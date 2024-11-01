import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ManufacturerService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-update-manufacturer',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-manufacturer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateManufacturerComponent implements OnInit {

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private manufacturerService = inject(ManufacturerService);
  private notificationService = inject(NotificationService);

  public manufacturerId!: string | null;
  public manufacturerForm!: FormGroup;

  public ngOnInit(): void {
    this.manufacturerId = this.route.snapshot.paramMap.get('id');
    this.manufacturerForm = this.formBuilder.group({
      manufacturerId: [this.manufacturerId],
      manufacturerName: ['', Validators.required]
    });
    this.manufacturerService.getManufacturerById(this.manufacturerId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.manufacturerForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(): void {
    if (this.manufacturerForm.invalid) {
      return;
    }
    this.manufacturerService.updateManufacturer(this.manufacturerId, this.manufacturerForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito',response.message);
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
