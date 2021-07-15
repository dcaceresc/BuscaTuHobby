import { Component, OnInit } from '@angular/core';
import { UniversesService } from 'src/app/core/services/universes.service';
import { Column } from 'src/app/shared/models/data-table-type';
import { UniverseVm } from 'src/app/shared/models/universe-vm';

@Component({
  templateUrl: './universe.component.html',
  styleUrls: ['./universe.component.scss']
})
export class UniverseComponent implements OnInit {

  universes!:UniverseVm[]
  universeColumns : Column<UniverseVm>[] = [
    {title:'Id',dataProperty:"id"},
    {title: "Nombre",dataProperty:"name"}
  ]

  constructor(
    private universesService:UniversesService
  ) {

  }

  ngOnInit(): void {
    this.universesService.Get().subscribe(items => this.universes = items);
  }

  onEdit(universe:UniverseVm){
    alert(universe.name);
  }
  onDelete(universe:UniverseVm){
    
  }

}
