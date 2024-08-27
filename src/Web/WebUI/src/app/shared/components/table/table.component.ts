import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [
    CommonModule,FontAwesomeModule
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
  public faEdit = faEdit;
  public faPowerOff = faPowerOff;
  public searchTerm :string = '';

  public onSearch(event: Event){
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.currentPage.set(1);
  }

  public setPage(pageNumber: number) {
    this.currentPage.set(pageNumber);
  }

  public nPage(){
    return Math.ceil(this.data()?.length / this.itemsPerPage)
  }

  public range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  public getFilteredData() {
  // Se espera que 'data' sea una función que devuelve un conjunto de datos
  return this.data()?.filter((item) => {
    // Para cada elemento ('item') en el conjunto de datos, se realiza la siguiente comprobación
    return Object.entries(item).some(([key, value]) => {
      // Se omite la columna 'IsActive' en la búsqueda
      if (key.toLowerCase() === 'isactive') {
        return false; // Ignorar la columna 'IsActive'
      }

      // Se convierte el valor actual a una cadena y se convierte a minúsculas
      // Luego se verifica si la cadena contiene el término de búsqueda (también convertido a minúsculas)
      return String(value).toLowerCase().includes(this.searchTerm.toLowerCase());
    });
  });
}

  public getPaginatedData(){
    const filteredData = this.getFilteredData();
    const startIndex = (this.currentPage() - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return filteredData?.slice(startIndex,endIndex);
  }


  public onEdit(id: number){
    this.editEvent.emit(id);
  }

  public onToggle(id: number) {
    this.toggleEvent.emit(id);
  }

}
