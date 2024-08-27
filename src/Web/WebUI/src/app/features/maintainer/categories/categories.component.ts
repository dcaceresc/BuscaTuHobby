import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [
    RouterLink,ButtonComponent,TableComponent
  ],
  templateUrl : './categories.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent implements OnInit{
  
  private router = inject(Router);

  public columns :any[] = [];
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'categoryId' },
      { name: 'Nombre', prop: 'categoryName' },
      { name: 'Grupo', prop: 'groupName' },
      { name: 'Acciones', prop: 'IsActive' },
    ];
  }

}
