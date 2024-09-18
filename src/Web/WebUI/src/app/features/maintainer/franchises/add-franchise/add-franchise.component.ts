import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FranchiseService } from '../../../../core/services/maintainer/franchise.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-add-franchise',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-franchise.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddFranchiseComponent implements OnInit { 

  private franchiseService = inject(FranchiseService);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  public franchiseForm!: FormGroup;

  public ngOnInit(){
    this.franchiseForm = this.formBuilder.group({
      franchiseName: ['',Validators.required],
    });
  }

  public onSubmit(){

    if(this.franchiseForm.invalid){
      return;
    }

    this.franchiseService.addFranchise(this.franchiseForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Success', response.message);
        this.router.navigate(['/maintainer/franchises']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });

  }

  public onCancel(){
    this.router.navigate(['/maintainer/franchises']);
  }
}
