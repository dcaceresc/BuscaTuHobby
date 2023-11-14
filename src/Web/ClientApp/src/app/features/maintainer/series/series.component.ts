import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';
import { serieDto } from 'src/app/core/models/serie.model';
import { SeriesService } from 'src/app/core/services/series.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './series.component.html',
  styleUrl: './series.component.scss'
})
export class SeriesComponent implements OnInit{
  series = signal<serieDto[]>([]);
  columns:any[] = [];

  constructor(private seriesService:SeriesService, private router:Router) {}

  ngOnInit(): void {
    this.seriesService.GetAll().subscribe(items => this.series.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'franchiseName', name: "Franquicia"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/series/edit/', id]);
  }


  onToggle(id:number){
    const serie = this.series().find(x => x.id === id);

    if(serie){
      this.seriesService.Toggle(id).subscribe(
        () => {
          this.seriesService.GetAll().subscribe(items => this.series.set(items));
        }
      );
    }
  }
}
