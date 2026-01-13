import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { DashboardMenuDto } from '@app/core/models';


@Component({
  selector: 'app-menu',
  imports: [NgClass],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuComponent{

  public readonly menu = input.required<DashboardMenuDto[]>();
  public expandedMenus: boolean[] = [];

  public toggleSubMenu(index: number): void {
    this.expandedMenus[index] = !this.expandedMenus[index];
  }

}
