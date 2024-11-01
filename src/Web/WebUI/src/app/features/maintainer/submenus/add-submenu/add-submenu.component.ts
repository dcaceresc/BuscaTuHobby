import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService, SubMenuService } from '@app/core/services';

@Component({
  selector: 'app-add-submenu',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-submenu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddScaleComponent implements OnInit { 

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private subMenuService = inject(SubMenuService);
  private notificationService = inject(NotificationService);

  public scaleForm!: FormGroup;

  public ngOnInit(): void {
    this.scaleForm = this.formBuilder.group({
      scaleName: ['', Validators.required]
    });
  }

  public onSubmit(): void {
    if (this.scaleForm.invalid) {
      return;
    }

    this.subMenuService.createSubMenu(this.scaleForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
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
