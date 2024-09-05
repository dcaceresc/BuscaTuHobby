import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './admin-login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLoginComponent { }