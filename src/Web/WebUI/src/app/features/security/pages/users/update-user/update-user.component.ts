import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-update-user',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './update-user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateUserComponent { }
