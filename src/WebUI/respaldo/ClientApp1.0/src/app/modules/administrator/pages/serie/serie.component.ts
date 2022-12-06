import { Component, OnInit } from '@angular/core';
import { SeriesService } from 'src/app/core/services/series.service';
import { Column } from 'src/app/shared/models/data-table-type';
import { SerieVm } from 'src/app/shared/models/serie-vm';


@Component({
  templateUrl: './serie.component.html',
  styleUrls: ['./serie.component.scss']
})
export class SerieComponent implements OnInit {

  series!:SerieVm[]
  serieColumns:Column<SerieVm>[] = [
    {title:'Id',dataProperty:"id"},
    {title:'Nombre',dataProperty:"name"},
    {title:'Universo',dataProperty:"universeId"}
  ]

  constructor(
    private seriesService:SeriesService
  ) { }

  ngOnInit(): void {
    this.seriesService.GetAll().subscribe(items => this.series = items);
  }

  onEdit(serie:SerieVm){
    alert(serie.name);
  }
  onDelete(serie:SerieVm){

  }

}
