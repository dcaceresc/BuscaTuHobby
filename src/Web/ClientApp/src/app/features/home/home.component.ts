import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PopularCategoriesComponent } from './components/popular-categories/popular-categories.component';
import { TodaysDealsComponent } from './components/todays-deals/todays-deals.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule,PopularCategoriesComponent,TodaysDealsComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

}
