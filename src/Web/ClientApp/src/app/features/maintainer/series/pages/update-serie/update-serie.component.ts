import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SeriesService } from 'src/app/core/services/series.service';
import { ActivatedRoute, Router } from '@angular/router';
import { serieVM } from 'src/app/core/models/serie.model';
import { franchiseDto } from 'src/app/core/models/franchise.model';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './update-serie.component.html',
  styleUrls: ['./update-serie.component.scss']
})
export class UpdateSerieComponent {
  serieId!:string | null;
  serieForm! : FormGroup;
  franchises = signal<franchiseDto[]>([])

  constructor(private seriesService: SeriesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private franchisesService : FranchisesService,
    private route: ActivatedRoute,) {
      this.serieId = this.route.snapshot.paramMap.get('id');
      this.loadFranchises();
  }

  ngOnInit(): void {
    this.serieForm = this.formBuilder.group({
      id:[this.serieId,Validators.required],
      name: ['', Validators.required],
      franchiseId: ['',Validators.required]
    });

    this.seriesService.GetbyId(this.serieId).subscribe(
      (serie) => {
        // Llena el formulario con los datos del ambiente
        this.serieForm.patchValue({
          name: serie.name,
          franchiseId:serie.franchiseId
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
  }


  loadFranchises(){
    this.franchisesService.GetAll().subscribe(
      (franchises) => {
        this.franchises.set(franchises.filter(frachise => frachise.active));
      }
    )
  }

  onSubmit(): void {
    if (this.serieForm.valid) {
      this.seriesService.Update(this.serieId,this.serieForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/series']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/series']);
  }
}
