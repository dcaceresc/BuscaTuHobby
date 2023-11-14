import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { categoryDto } from 'src/app/core/models/category.model';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink, TableComponent],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit{

  categories = signal<categoryDto[]>([]);
  columns:any[] = [];

  constructor(private categoriesService:CategoriesService, private router :Router) {}

  ngOnInit(): void {
    this.categoriesService.GetAll().subscribe(items => this.categories.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'groupName', name: "Grupo"},
      {key: 'active', name : "Acciones"}
    ]  
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/categories/edit/', id]);
  }

  onToggle(id: number) {
    const category = this.categories().find(x => x.id === id);

    if(category){
      this.categoriesService.Toggle(id).subscribe(
        () => {
          this.categoriesService.GetAll().subscribe(items => this.categories.set(items));
        }
      );
    }
  }
}
