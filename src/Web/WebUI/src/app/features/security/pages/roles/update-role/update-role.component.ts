import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-update-role',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './update-role.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateRoleComponent { }
