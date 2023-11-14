import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { inventoryDto } from 'src/app/core/models/inventory.model';
import { InventoriesService } from 'src/app/core/services/inventories.service';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './inventories.component.html',
  styleUrl: './inventories.component.scss'
})
export class InventoriesComponent implements OnInit{
  inventories = signal<inventoryDto[]>([]);
  columns:any[] = [];

  constructor(private inventoriesService:InventoriesService,private router:Router) {}

  ngOnInit(): void {
    this.inventoriesService.GetAll().subscribe(items => this.inventories.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'productName', name: "Producto"},
      {key: 'storeName', name: "Tienda"},
      {key: 'price', name: 'Precio'},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id: number) {
    this.router.navigate(['/maintainer/inventories/edit/', id]);
  }
  
  onToggle(id: number) {
    const inventory = this.inventories().find(x => x.id === id);
  
    if(inventory){
      this.inventoriesService.Toggle(id).subscribe(
        () => {
          this.inventoriesService.GetAll().subscribe(items => this.inventories.set(items));
        }
      );
    }
  }
}
