import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-administration-menu',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './administration-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdministrationMenuComponent { }
