import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-add-configuration',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './add-configuration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddConfigurationComponent { }
