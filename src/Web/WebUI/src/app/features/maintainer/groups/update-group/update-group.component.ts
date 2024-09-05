import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GroupService } from '../../../../core/services/maintainer/group.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-update-group',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './update-group.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateGroupComponent implements OnInit{

  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formBuilder = inject(FormBuilder);
  private groupService = inject(GroupService);
  private notificationService = inject(NotificationService);

  public groupId!: string | null;
  public groupForm!: FormGroup;

  public ngOnInit(): void {
    this.groupId = this.route.snapshot.paramMap.get('id');
    this.groupForm = this.formBuilder.group({
      groupId: [this.groupId],
      groupName: ['', Validators.required]
    });
    this.groupService.getGroupById(this.groupId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.groupForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }


  public onSubmit(): void {
    if (this.groupForm.invalid) {
      return;
    }
    this.groupService.updateGroup(this.groupId, this.groupForm.value).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito',response.message);
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
