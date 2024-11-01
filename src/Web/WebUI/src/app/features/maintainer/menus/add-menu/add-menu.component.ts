import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MenuService, NotificationService } from '@app/core/services';

@Component({
  selector: 'app-add-group',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddMenuComponent implements OnInit { 

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private menuService = inject(MenuService);
  private notificationService = inject(NotificationService);

  public menuForm!: FormGroup;

  public ngOnInit(): void {
    this.menuForm = this.formBuilder.group({
      menuName: ['', Validators.required],
      menuOrder: ['', Validators.required],
    });
  }

  public onSubmit(): void {
    if (this.menuForm.invalid) {
      return;
    }

    this.menuService.createMenu(this.menuForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/menus']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/menus']);
  }
  
}
