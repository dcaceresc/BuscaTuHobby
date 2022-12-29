import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class TableComponent {
  @Input() data: any[] | undefined;
  @Input() columns: string[] | undefined;

  pageSize: number = 10;
  page:number = 1;
}
