import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { TableComponent } from 'src/app/shared/components/table/table.component';
import { scaleDto } from 'src/app/core/models/scale.model';
import { ScalesService } from 'src/app/core/services/scales.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,TableComponent],
  templateUrl: './scales.component.html',
  styleUrl: './scales.component.scss'
})
export class ScalesComponent implements OnInit{
  scales = signal<scaleDto[]>([]);
  columns:any[] = [];

  constructor(private scaleService:ScalesService, private router:Router) {}

  ngOnInit(): void {
    this.scaleService.GetAll().subscribe(items => this.scales.set(items));
    this.columns = [
      {key: 'id', name : '#'},
      {key: 'name', name: "Nombre"},
      {key: 'active', name : "Acciones"}
    ]
  }

  onEdit(id:number){
    this.router.navigate(['/maintainer/scales/edit/', id]);
  }

  onToggle(id:number){
    const scale = this.scales().find(x => x.id === id);

    if(scale){
      this.scaleService.Toggle(id).subscribe(
        () => {
          this.scaleService.GetAll().subscribe(items => this.scales.set(items));
        }
      );
    }
  }
}
