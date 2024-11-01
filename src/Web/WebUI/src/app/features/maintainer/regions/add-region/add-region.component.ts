import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService, RegionService } from '@app/core/services';

@Component({
  selector: 'app-add-region',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-region.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddRegionComponent implements OnInit { 

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);

  public regionForm!: FormGroup;

  public ngOnInit() : void {
    this.regionForm = this.formBuilder.group({
      regionName: ['',Validators.required],
      regionOrder: ['',Validators.required]
    });
  }


  public onSubmit() {
    if (this.regionForm.invalid) {
      return;
    }

    this.regionService.createRegion(this.regionForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.notificationService.showSuccess('Exito', response.message);
        this.router.navigate(['/maintainer/regions']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onCancel() {
    this.router.navigate(['/maintainer/regions']);
  }
}
