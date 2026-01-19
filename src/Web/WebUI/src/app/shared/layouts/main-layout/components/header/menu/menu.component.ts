import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MenuCategoryDto } from '@app/core/models/dashboard/dashboard.model';
import { DashboardService, NotificationService } from '@app/core/services';
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';


@Component({
  selector: 'app-menu',
  imports: [RouterLink,RouterLinkActive,NgClass],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuComponent implements OnInit {

  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);
  activeOffcanvas = inject(NgbActiveOffcanvas);

  public categories = signal<MenuCategoryDto[]>([]);
  public showCategories: boolean = false;
  public showStores: boolean = false;

  public showCategoriesMenu(): void {
    this.showCategories = !this.showCategories;
  }

  public showStoresMenu(): void {
    this.showStores = !this.showStores;
  }

  public ngOnInit(): void {
    this.dashboardService.getMenuCategories().subscribe({
      next: (response) => {

        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.categories.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

}
