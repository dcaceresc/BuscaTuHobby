import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { franchiseDto } from 'src/app/core/models/franchise.model';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './franchises.component.html',
  styleUrl: './franchises.component.scss'
})
export class FranchisesComponent implements OnInit{
  franchises = signal<franchiseDto[]>([]);
  columns:any[] = [];

  constructor(private franchisesService:FranchisesService,private router :Router) {}

  ngOnInit(): void {
    this.franchisesService.GetAll().subscribe(items => this.franchises.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id: number) {
    this.router.navigate(['/maintainer/franchises/edit/', id]);
  }

  onToggle(id: number) {
    const group = this.franchises().find(x => x.id === id);
  
    if(group){
      this.franchisesService.Toggle(id).subscribe(
        () => {
          this.franchisesService.GetAll().subscribe(items => this.franchises.set(items));
        }
      );
    }
  }
}
