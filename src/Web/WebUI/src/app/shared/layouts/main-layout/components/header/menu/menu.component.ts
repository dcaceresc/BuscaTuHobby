import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { DashboardMenuDto } from '@app/core/models';
import { FaIconService } from '@app/core/services';
import { FontAwesomeModule, IconDefinition } from '@fortawesome/angular-fontawesome';
;

@Component({
  selector: 'app-menu',
  imports: [FontAwesomeModule],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuComponent{

  private faIconService = inject(FaIconService);

  public readonly menu = input.required<DashboardMenuDto[]>();
  public expandedMenus: boolean[] = [];

  public toggleSubMenu(index: number): void {
    this.expandedMenus[index] = !this.expandedMenus[index];
  }

  public getIcon(iconName: string): IconDefinition {
    return this.faIconService.getIcon(iconName);
  }
}
