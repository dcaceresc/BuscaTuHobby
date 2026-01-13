
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { HeaderComponent } from './components/header/header.component';
import { RouterOutlet } from '@angular/router';

@Component({
    selector: 'app-frontend-layout',
    imports: [
        RouterOutlet, HeaderComponent
    ],
    template: `<app-header />
            <div class="container-fluid">
              <router-outlet/>
            </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayoutComponent { }
