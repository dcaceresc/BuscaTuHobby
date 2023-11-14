import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { storeDto } from 'src/app/core/models/store.model';
import { StoresService } from 'src/app/core/services/stores.service';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './stores.component.html',
  styleUrl: './stores.component.scss'
})
export class StoresComponent implements OnInit{
  stores = signal<storeDto[]>([]);
  columns:any[] = [];

  constructor(private storesService: StoresService, private router:Router) { }

  ngOnInit(): void {
    this.storesService.GetAll().subscribe(items => this.stores.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'address', name: "Dirección"},
      {key: 'webSite', name: "WebSite"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/stores/edit/', id]);
  }

  onToggle(id:number){
    const store = this.stores().find(x => x.id === id);

    if(store){
      this.storesService.Toggle(id).subscribe(
        () => {
          this.storesService.GetAll().subscribe(items => this.stores.set(items));
        }
      );
    }
  }
}
