import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SeriesService } from 'src/app/core/services/series.service';
import { universeVM } from 'src/app/core/models/universe.model';
import { UniversesService } from 'src/app/core/services/universes.service';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-serie.component.html',
  styleUrls: ['./add-serie.component.scss']
})
export class AddSerieComponent {
  serieForm! : FormGroup;
  universes! : universeVM[];

  constructor(private formbuilder: FormBuilder,
              private seriesService: SeriesService,
              private universesService:UniversesService,
              private router:Router) {
    this.createForm();
    this.loadUniverses();
  }

  createForm() {
    this.serieForm = this.formbuilder.group({
      name: ['',Validators.required],
      universeId : ['',Validators.required]
    });
  }

  loadUniverses(){
    this.universesService.GetAll().subscribe(
      (universes) => {
        this.universes = universes.filter(universe => universe.active);
      }
    );
  }

  onSubmit():void{
    if(this.serieForm.valid){
      this.seriesService.Create(this.serieForm.value).subscribe(() => {
        this.router.navigate(['maintainer/series']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/series']);
  }
}
