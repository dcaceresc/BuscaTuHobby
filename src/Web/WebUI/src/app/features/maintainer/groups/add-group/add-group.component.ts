import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { GroupService } from '../../../../core/services/maintainer/group.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-add-group',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-group.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddGroupComponent implements OnInit { 

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private groupService = inject(GroupService);
  private notificationService = inject(NotificationService);

  public groupForm!: FormGroup;

  public ngOnInit(): void {
    this.groupForm = this.formBuilder.group({
      groupName: ['', Validators.required]
    });
  }

  public onSubmit(): void {
    if (this.groupForm.invalid) {
      return;
    }

    this.groupService.addGroup(this.groupForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/groups']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/groups']);
  }
  
}
