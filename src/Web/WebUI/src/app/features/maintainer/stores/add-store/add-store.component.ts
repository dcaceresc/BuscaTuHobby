import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { StoreService } from '../../../../core/services/maintainer/store.service';

@Component({
  selector: 'app-add-store',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-store.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddStoreComponent implements OnInit{

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);

  public storeForm!: FormGroup;

  public ngOnInit(): void {
    this.storeForm = this.formBuilder.group({
      storeName: ['', Validators.required],
      storeAddress: ['', Validators.required],
      storeWebSite: ['', Validators.required]
    });
  }

  public onSubmit(): void {
    if (this.storeForm.invalid) {
      return;
    }

    this.storeService.createStore(this.storeForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/stores']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/stores']);
  }

}
