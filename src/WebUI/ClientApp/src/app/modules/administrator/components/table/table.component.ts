import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Column } from 'src/app/shared/models/data-table-type';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class TableComponent<T> implements OnInit {

  @Input() title!:string
  @Input() columns!: Column<T>[];
  @Input() rows!: T[];
  @Output() newEvent = new EventEmitter();


  constructor() { }

  ngOnInit(): void {
  }

  newItem(){
    this.newEvent.emit();
  }

}
