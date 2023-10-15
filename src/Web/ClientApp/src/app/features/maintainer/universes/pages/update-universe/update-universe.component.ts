import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { universeVM } from 'src/app/core/models/universe.model';
import { UniversesService } from 'src/app/core/services/universes.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-universe.component.html',
  styleUrls: ['./update-universe.component.scss']
})
export class UpdateUniverseComponent {
  universeId!:string | null;
  universe!: universeVM;
  universeForm! : FormGroup;

  constructor(private universeService: UniversesService,
              private router: Router,
              private formBuilder: FormBuilder,
              private route: ActivatedRoute,) {
                this.universeId = this.route.snapshot.paramMap.get('id');
  }
  ngOnInit(): void {
    this.universeForm = this.formBuilder.group({
      id:[this.universeId,Validators.required],
      name: ['', Validators.required],
      acronym : ['',Validators.required],
    });

    this.universeService.GetbyId(this.universeId).subscribe(
      (universe) => {
        // Llena el formulario con los datos del ambiente
        this.universeForm.patchValue({
          name: universe.name,
          acronym : universe.acronym,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de universos', error);
      }
    );
  }

  onSubmit(): void {
    if (this.universeForm.valid) {
      this.universeService.Update(this.universeId,this.universeForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/universes']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }







  onCancel(): void {
    this.router.navigate(['/maintainer/universes']);
  }
}
