import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { SeriesService } from 'src/app/core/services/series.service';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { serieVM } from 'src/app/core/models/serie.model';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-series.component.html',
  styleUrls: ['./list-series.component.scss']
})
export class ListSeriesComponent {
  series!:serieVM[];
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = 1;
  itemsPerPage = 10;

  constructor(private seriesService:SeriesService) {}

  ngOnInit(): void {
    this.seriesService.GetAll().subscribe(items => this.series = items);
  }

  getPaginatedData(){
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.series?.slice(startIndex,endIndex);
  }

  setPage(pageNumber: number) {
    this.currentPage = pageNumber;
  }

  range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  nPage(){
    return Math.ceil(this.series?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const universe = this.series.find(x => x.id === id);

    if(universe){
      this.seriesService.Toggle(id).subscribe(
        () => {
          this.seriesService.GetAll().subscribe(items => this.series = items);
        }
      );
    }
  }
}
