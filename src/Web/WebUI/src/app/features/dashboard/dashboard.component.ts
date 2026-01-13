
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RecentPostsComponent } from './recent-posts/recent-posts.component';
import { MostSearchedProductsComponent } from './most-searched-products/most-searched-products.component';
import { BestDealsComponent } from "./best-deals/best-deals.component";
import { PopularCategoriesComponent } from './popular-categories/popular-categories.component';
import { FeaturedStoresComponent } from './featured-stores/featured-stores.component';
import { RecentActivityComponent } from './recent-activity/recent-activity.component';
import { SearchProductComponent } from "./search-product/search-product.component";

@Component({
    selector: 'app-dashboard',
    imports: [
    RecentPostsComponent,
    MostSearchedProductsComponent,
    BestDealsComponent,
    PopularCategoriesComponent,
    FeaturedStoresComponent,
    RecentActivityComponent,
    SearchProductComponent
],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent { }
