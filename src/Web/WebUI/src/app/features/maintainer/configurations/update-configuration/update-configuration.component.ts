import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-update-configuration',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './update-configuration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateConfigurationComponent { }
