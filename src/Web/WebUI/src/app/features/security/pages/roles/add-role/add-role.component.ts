import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-add-role',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './add-role.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddRoleComponent { }
