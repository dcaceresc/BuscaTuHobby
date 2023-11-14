import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { productDto } from 'src/app/core/models/product.model';
import { ProductsService } from 'src/app/core/services/products.service';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit{
  products = signal<productDto[]>([]);
  columns:any[] = [];

  constructor(private productsService: ProductsService, private router:Router) { }

  ngOnInit(): void {
    this.productsService.GetAll().subscribe(items => this.products.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'scaleName', name: "Escala"},
      {key: 'manufacturerName', name: "Fabricante"},
      {key: 'franchiseName', name: "Franquicia"},
      {key: 'serieName', name: "Serie"},
      {key: 'categories',name:'Categorias'},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/products/edit/', id]);
  }

  onToggle(id:number){
    const store = this.products().find(x => x.id === id);

    if(store){
      this.productsService.Toggle(id).subscribe(
        () => {
          this.productsService.GetAll().subscribe(items => this.products.set(items));
        }
      );
    }
  }
}
