import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SeriesService } from 'src/app/core/services/series.service';
import { NgSelectModule } from '@ng-select/ng-select';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { franchiseDto } from 'src/app/core/models/franchise.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-serie.component.html',
  styleUrls: ['./add-serie.component.scss']
})
export class AddSerieComponent {
  serieForm! : FormGroup;
  franchises = signal<franchiseDto[]>([]);

  constructor(private formbuilder: FormBuilder,
              private seriesService: SeriesService,
              private franchisesService : FranchisesService,
              private router:Router) {
    this.createForm();
    this.loadFranchises();
  }

  createForm() {
    this.serieForm = this.formbuilder.group({
      name: ['',Validators.required],
      franchiseId: ['',Validators.required]
    });
  }

  loadFranchises(){
    this.franchisesService.GetAll().subscribe(
      (franchises) => {
        this.franchises.set(franchises.filter(frachise => frachise.active));
      }
    )
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
