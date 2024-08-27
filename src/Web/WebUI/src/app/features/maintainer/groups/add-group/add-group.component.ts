import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-add-group',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './add-group.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddGroupComponent { }
