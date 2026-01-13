import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, input, output } from '@angular/core';

@Component({
    selector: 'app-table',
    imports: [CommonModule],
    templateUrl: './table.component.html',
    styleUrl: './table.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableComponent {

  readonly data = input(signal<any[]>([]));
  readonly columns = input<any[]>([]);
  readonly actions = input<{
    icon: string;
    label: string;
    actionKey: string;
    cssClass: string;
}[]>([]); // Acciones personalizables
  readonly actionEvent = output<{
    id: string;
    actionKey: string;
}>();

  public currentPage = signal(1);
  public readonly itemsPerPage = 10;
  public searchTerm :string = '';

  public onSearch(event: Event){
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.currentPage.set(1);
  }

  public setPage(pageNumber: number) {
    this.currentPage.set(pageNumber);
  }

  public nPage(){
    return Math.ceil(this.data()()?.length / this.itemsPerPage)
  }

  public range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  public getFilteredData() {
  return this.data()()?.filter((item) => {
    return Object.entries(item).some(([key, value]) => {
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

  public onAction(id: string, actionKey: string) {
    this.actionEvent.emit({ id, actionKey });
  }


  public getTypedValue(value: any) : string {
    if (typeof value === 'boolean') {
      return "boolean";
    } else if (Array.isArray(value)) {
      return "array";
    } else if (typeof value === 'number') {
      return "number";
    }

    return value;
  }

  

}
