import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UniverseService } from 'src/app/core/services/universe.service';
import { Column } from 'src/app/shared/models/data-table-type';
import { UniverseVm } from 'src/app/shared/models/universe-vm';
import { ModalUniverseComponent } from '../../components/modal-universe/modal-universe.component';

@Component({
  templateUrl: './universes.component.html',
  styleUrls: ['./universes.component.scss']
})
export class UniversesComponent implements OnInit {

  universes!:UniverseVm[];
  universe!:UniverseVm
  universeColumns:Column<UniverseVm>[] = [
    {title:'Id',dataProperty:"id"},
    {title:'Nombre',dataProperty:"name"}
  ]

  constructor(
    private universeService:UniverseService,
    public modalService:NgbModal
  ) { }

  ngOnInit(): void {
    this.universeService.GetAll().subscribe(items => this.universes = items);
  }

  onNew(){
    const modalRef = this.modalService.open(ModalUniverseComponent,{ size: 'lg' })
    modalRef.componentInstance.header = "Nuevo Universo";
  }

}
