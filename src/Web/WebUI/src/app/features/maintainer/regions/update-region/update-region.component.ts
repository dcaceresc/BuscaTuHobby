import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RegionService } from '../../../../core/services/maintainer/region.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-update-region',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-region.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateRegionComponent implements OnInit{ 

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);

  public regionId!: string | null;
  public regionForm!: FormGroup;

  public ngOnInit() : void {
    this.regionId = this.route.snapshot.paramMap.get('id');
    this.regionForm = this.formBuilder.group({
      regionId: [this.regionId],
      regionName: ['',Validators.required],
      regionOrder: ['',Validators.required]
    });

    this.regionService.getRegionById(this.regionId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.regionForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }


  public onSubmit() {
    if (this.regionForm.invalid) {
      return;
    }

    this.regionService.updateRegion(this.regionId,this.regionForm.value).subscribe({
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
    return this.router.navigate(['/maintainer/regions']);
  }

}
