import { ChangeDetectionStrategy, Component, inject, OnInit, input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FranchiseService, NotificationService } from '@app/core/services';

@Component({
    selector: 'app-add-edit-franchise',
    imports: [ReactiveFormsModule],
    templateUrl: './add-edit-franchise.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditFranchiseComponent implements OnInit {
  private franchiseService = inject(FranchiseService);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  readonly franchiseId = input.required<string | null>({ alias: "id" });
  public isEditMode : boolean = false;
  public franchiseForm!: FormGroup;

  public ngOnInit(){
    this.isEditMode = !!this.franchiseId();
    this.createForm();

    if(this.isEditMode){
      this.franchiseService.getFranchise(this.franchiseId()).subscribe({
        next: (response) => {
          if (!response.success) {
            this.notificationService.showError('Error', response.message);
            return;
          }
          this.franchiseForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(){
    if(this.isEditMode){
      this.franchiseForm = this.formBuilder.group({
        franchiseId: [this.franchiseId(),Validators.required],
        franchiseName: ['',Validators.required],
      });
    }else{
      this.franchiseForm = this.formBuilder.group({
        franchiseName: ['',Validators.required],
      });
    }
  }

  public onSubmit(){

    if(this.franchiseForm.invalid){
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.franchiseService.updateFranchise(this.franchiseId(), this.franchiseForm.value).subscribe({
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
    }else{
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
  }

  public onCancel(){
    this.router.navigate(['/maintainer/franchises']);
  }
}
