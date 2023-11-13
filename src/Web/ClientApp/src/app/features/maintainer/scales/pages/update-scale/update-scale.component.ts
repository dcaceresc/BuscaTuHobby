import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { scaleVM } from 'src/app/core/models/scale.model';
import { ScalesService } from 'src/app/core/services/scales.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-scale.component.html',
  styleUrls: ['./update-scale.component.scss']
})
export class UpdateScaleComponent {
  scaleId!:string | null;
  scaleForm! : FormGroup;

  constructor(private scalesService: ScalesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.scaleId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.scaleForm = this.formBuilder.group({
      id:[this.scaleId,Validators.required],
      name: ['', Validators.required],
    });

    this.scalesService.GetbyId(this.scaleId).subscribe(
      (scale) => {
        // Llena el formulario con los datos del ambiente
        this.scaleForm.patchValue({
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
    if (this.scaleForm.valid) {
      this.scalesService.Update(this.scaleId,this.scaleForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/scales']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/scales']);
  }
}

