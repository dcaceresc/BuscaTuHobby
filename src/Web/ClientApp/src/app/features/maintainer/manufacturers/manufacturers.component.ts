import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';
import { manufacturerDto } from 'src/app/core/models/manufacturer.model';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './manufacturers.component.html',
  styleUrl: './manufacturers.component.scss'
})
export class ManufacturersComponent implements OnInit{
  manufacturers = signal<manufacturerDto[]>([]);
  columns:any[] = [];

  constructor(private manufacturersService:ManufacturersService, private router:Router) {}

  ngOnInit(): void {
    this.manufacturersService.GetAll().subscribe(items => this.manufacturers.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/manufacturers/edit/', id]);
  }

  onToggle(id:number){
    const manufacturer = this.manufacturers().find(x => x.id === id);

    if(manufacturer){
      this.manufacturersService.Toggle(id).subscribe(
        () => {
          this.manufacturersService.GetAll().subscribe(items => this.manufacturers.set(items));
        }
      );
    }
  }
}
