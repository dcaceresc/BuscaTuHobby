import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, input, output, effect } from '@angular/core';

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
  }[]>([]);
  readonly searchTerm = input<string>('');
  readonly actionEvent = output<{
      id: string;
      actionKey: string;
  }>();

  public currentPage = signal(1);
  public readonly itemsPerPage = 10;
  public maxPagesToShow = 5; // Máximo de páginas a mostrar a la izquierda y derecha


  constructor() {
    effect(() => {
      // 👇 Cada vez que cambian los datos o el searchTerm
      this.data()();
      this.searchTerm();

      // 🔥 volvemos SIEMPRE a la página 1
      this.currentPage.set(1);
    });
  }

  public setPage(pageNumber: number | string) {
    if (typeof pageNumber === 'string') {
      return;
    }
    this.currentPage.set(pageNumber);
  }

  public nPage(){
    const filteredData = this.getFilteredData();
    return Math.ceil(filteredData.length / this.itemsPerPage);
  }

  public range() {

    const totalPages = this.nPage();
    const currentPage = this.currentPage();
    const maxPages = this.maxPagesToShow;
    const pages = [];

    let startPage: number;
    let endPage: number;

    if (totalPages <= maxPages) {
      // Si el número total de páginas es menor o igual al máximo, mostrar todas las páginas
      startPage = 1;
      endPage = totalPages;
    } else {
      // Calcular las páginas a mostrar alrededor de la página actual
      const halfMaxPages = Math.floor(maxPages / 2);


  
      if (currentPage <= halfMaxPages) {
        // Mostrar desde la primera página si la página actual está cerca del inicio
        startPage = 1;
        endPage = maxPages;
      } else if (currentPage + halfMaxPages >= totalPages) {
        // Mostrar hasta la última página si la página actual está cerca del final
        startPage = totalPages - maxPages + 1;
        endPage = totalPages;
      } else {
        // Mostrar páginas alrededor de la página actual
        startPage = currentPage - halfMaxPages;
        endPage = currentPage + halfMaxPages;
      }
    }
  
    // Agregar primera página y "..."
    if (startPage > 1) {
      pages.push(1);
      if (startPage > 2) {
        pages.push('...');
      }
    }
  
    // Agregar páginas dentro del rango calculado
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
  
    // Agregar última página y "..."
    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        pages.push('...');
      }
      pages.push(totalPages);
    }
  
    return pages;
  }

  public getFilteredData() {
  return this.data()()?.filter((item) => {
    return Object.entries(item).some(([key, value]) => {
      if (key.toLowerCase() === 'isactive') {
        return false; // Ignorar la columna 'IsActive'
      }

      // Se convierte el valor actual a una cadena y se convierte a minúsculas
      // Luego se verifica si la cadena contiene el término de búsqueda (también convertido a minúsculas)
      return String(value).toLowerCase().includes(this.searchTerm().toLowerCase());
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
