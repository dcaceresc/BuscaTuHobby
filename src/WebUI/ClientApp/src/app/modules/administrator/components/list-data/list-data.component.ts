import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { faSort } from '@fortawesome/free-solid-svg-icons';

import { Column } from 'src/app/shared/models/data-table-type';

@Component({
  selector: 'app-list-data',
  templateUrl: './list-data.component.html',
  styleUrls: ['./list-data.component.scss']
})

export class ListDataComponent<T> implements OnInit, OnChanges{

  @Input() columns!: Column<T>[];
  @Input() rows!: T[];
  @Output() editEvent = new EventEmitter<T>()
  @Output() deleteEvent = new EventEmitter<T>()
  currentPage:number = 1;
  itemsPerPage:number = 8;
  key:string = 'id';
  reverse:boolean = false;
  caseInsensitive:boolean = true;
  faSort=faSort;
  term!:string;



  ngOnInit(): void {




  }



  ngOnChanges(changes: SimpleChanges): void {

  }

  sort(key:string){
    this.key = key;
    this.reverse = !this.reverse;
  }

  editItem(item:T){
    this.editEvent.emit(item);
  }

  deleteItem(item:T){
    this.deleteEvent.emit(item)
  }



}


