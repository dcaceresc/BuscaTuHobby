import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SeriesService } from 'src/app/core/services/series.service';
import { ActivatedRoute, Router } from '@angular/router';
import { serieVM } from 'src/app/core/models/serie.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-serie.component.html',
  styleUrls: ['./update-serie.component.scss']
})
export class UpdateSerieComponent {
  serieId!:string | null;
  serie!: serieVM;
  serieForm! : FormGroup;

  constructor(private seriesService: SeriesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.serieId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.serieForm = this.formBuilder.group({
      id:[this.serieId,Validators.required],
      name: ['', Validators.required],
    });

    this.seriesService.GetbyId(this.serieId).subscribe(
      (scale) => {
        // Llena el formulario con los datos del ambiente
        this.serieForm.patchValue({
          name: scale.name,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
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
