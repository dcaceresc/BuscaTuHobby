import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MenuService } from '../../../../core/services/maintainer/menu.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-update-group',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateGroupComponent implements OnInit{

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private menuService = inject(MenuService);
  private notificationService = inject(NotificationService);

  public menuId!: string | null;
  public menuForm!: FormGroup;

  public ngOnInit(): void {
    this.menuId = this.route.snapshot.paramMap.get('id');
    this.menuForm = this.formBuilder.group({
      menuId: [this.menuId],
      menuName: ['', Validators.required],
      menuOrder: ['', Validators.required],
    });
    this.menuService.getMenuById(this.menuId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.menuForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(): void {
    if (this.menuForm.invalid) {
      return;
    }
    this.menuService.updateMenu(this.menuId, this.menuForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito',response.message);
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
