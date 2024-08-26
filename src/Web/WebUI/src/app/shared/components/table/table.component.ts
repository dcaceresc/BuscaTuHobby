import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal } from '@angular/core';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './table.component.html',
  styleUrl: './table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableComponent {

  @Input() data = signal<any[]>([]);
  @Input() columns:any[] = []
  @Output() editEvent : EventEmitter<number> = new EventEmitter<number>();
  @Output() toggleEvent : EventEmitter<number> = new EventEmitter<number>();

  public currentPage = signal(1);
  public readonly itemsPerPage = 10;
  // public faEdit = faEdit;
  // public faPowerOff = faPowerOff;
  public searchTerm :string = '';

  public onSearch(event: Event){
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.currentPage.set(1);
  }

}
