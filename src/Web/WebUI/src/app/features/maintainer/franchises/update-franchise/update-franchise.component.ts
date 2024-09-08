import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FranchiseService } from '../../../../core/services/maintainer/franchise.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-update-franchise',
  standalone: true,
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './update-franchise.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateFranchiseComponent implements OnInit { 
  
  private franchiseService = inject(FranchiseService);
  private formBuilder = inject(FormBuilder);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  public franchiseForm!: FormGroup;
  public franchiseId!: string | null;

  public ngOnInit(): void {

    this.franchiseId = this.route.snapshot.paramMap.get('id');

    this.franchiseForm = this.formBuilder.group({
      franchiseId : [this.franchiseId],
      franchiseName: ['',Validators.required],
    });

    this.franchiseService.getFranchise(this.franchiseId).subscribe({
      next: (response) => {

        if (!response.success) {
          this.notificationService.showDefaultError();
          return;
        }

        this.franchiseForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });

    
  }

  public onSubmit(): void {

    if(this.franchiseForm.invalid){
      return;
    }

    this.franchiseService.updateFranchise(this.franchiseId,this.franchiseForm.value).subscribe({
      next: (response) => {

        if (!response.success) {
          this.notificationService.showError("Error", response.message);  
          return;
        }

        this.router.navigate(['/maintainer/franchises']);
        this.notificationService.showSuccess('Exito', response.message);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });

  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/franchises']);
  }
}
