import { Component } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UniverseService } from 'src/app/core/services/universe.service';
import { UniverseVm } from 'src/app/shared/models/universe-vm';
import { CreateComponent } from '../../components/create/create.component';


@Component({
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.scss']
})
export class IndexComponent {
  universes!:UniverseVm[];


  constructor(private modalService : NgbModal, private universeService : UniverseService){

  }

  ngOnInit(): void {
    this.universeService.GetAll().subscribe(items => this.universes = items);
  }


  open(){
    this.modalService.open(CreateComponent);
  }
}
