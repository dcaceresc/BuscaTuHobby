import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { ScaleService } from '../../../../core/services/maintainer/scale.service';

@Component({
  selector: 'app-update-scale',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-scale.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateScaleComponent implements OnInit {

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private scaleService = inject(ScaleService);
  private notificationService = inject(NotificationService);

  public scaleId!: string | null;
  public scaleForm!: FormGroup;

  public ngOnInit(): void {
    this.scaleId = this.route.snapshot.paramMap.get('id');
    this.scaleForm = this.formBuilder.group({
      scaleId: [this.scaleId],
      scaleName: ['', Validators.required]
    });
    this.scaleService.getScaleById(this.scaleId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.scaleForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(): void {
    if (this.scaleForm.invalid) {
      return;
    }
    this.scaleService.updateScale(this.scaleId, this.scaleForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito',response.message);
        this.router.navigate(['/maintainer/scales']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/scales']);
  }
}
