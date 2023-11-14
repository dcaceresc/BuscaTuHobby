import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule,FontAwesomeModule],
  templateUrl: './table.component.html',
  styleUrl: './table.component.scss'
})
export class TableComponent {
  @Input() data = signal<any[]>([]);
  @Input() columns:any[] = []
  @Output() editEvent : EventEmitter<number> = new EventEmitter<number>();
  @Output() toggleEvent : EventEmitter<number> = new EventEmitter<number>();
  currentPage = signal(1);
  readonly itemsPerPage = 10;

  faEdit = faEdit;
  faPowerOff = faPowerOff;

  setPage(pageNumber: number) {
    this.currentPage.set(pageNumber);
  }

  nPage(){
    return Math.ceil(this.data()?.length / this.itemsPerPage)
  }

  range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  getPaginatedData(){
    const startIndex = (this.currentPage() - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.data()?.slice(startIndex,endIndex);
  }


  onEdit(id: number){
    this.editEvent.emit(id);
  }

  onToggle(id: number) {
    this.toggleEvent.emit(id);
  }

}
