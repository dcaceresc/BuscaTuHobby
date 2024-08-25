import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { HeaderComponent } from './components/header/header.component';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-frontend-layout',
  standalone: true,
  imports: [
    RouterOutlet,HeaderComponent
  ],
  template: `<app-header />
            <div class="container-fluid">
              <router-outlet/>
            </div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent { }
