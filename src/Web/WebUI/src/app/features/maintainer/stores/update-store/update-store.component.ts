import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { StoreService } from '../../../../core/services/maintainer/store.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-update-store',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-store.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateStoreComponent implements OnInit{ 

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);

  public storeForm!: FormGroup;
  public storeId!: string | null;

  public ngOnInit(): void {

    this.storeId = this.route.snapshot.paramMap.get('id');

    this.storeForm = this.formBuilder.group({
      storeId: [this.storeId],
      storeName: ['', Validators.required],
      storeAddress: ['', Validators.required],
      storeWebSite: ['', Validators.required]
    });

    this.storeService.getStoreById(this.storeId).subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.storeForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onSubmit(){
    if(this.storeForm.invalid){
      return;
    }

    this.storeService.updateStore(this.storeId, this.storeForm.value).subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Success", response.message);
        this.router.navigate(['/maintainer/stores']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(){
    this.router.navigate(['/maintainer/stores']);
  }
}
