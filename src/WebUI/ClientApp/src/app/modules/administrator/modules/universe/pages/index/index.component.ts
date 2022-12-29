import { Component } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CreateComponent } from '../../components/create/create.component';

@Component({
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.scss']
})
export class IndexComponent {

  constructor(private modalService : NgbModal){

  }


  products = [
    { id: 1, name: 'Product 1', price: 10.99, category: 'Category 1' },
    { id: 2, name: 'Product 2', price: 5.99, category: 'Category 2' },
    { id: 3, name: 'Product 3', price: 15.99, category: 'Category 1' },
    { id: 4, name: 'Product 4', price: 8.99, category: 'Category 2' },
    { id: 5, name: 'Product 5', price: 12.99, category: 'Category 3' }
  ];


  open(){
    this.modalService.open(CreateComponent);
  }
}
