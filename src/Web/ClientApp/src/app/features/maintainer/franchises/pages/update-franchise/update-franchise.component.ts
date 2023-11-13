import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-franchise.component.html',
  styleUrls: ['./update-franchise.component.scss']
})
export class UpdateFranchiseComponent {
  franchiseId!:string | null;
  franchiseForm! : FormGroup;

  constructor(private franchisesService: FranchisesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.franchiseId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.franchiseForm = this.formBuilder.group({
      id:[this.franchiseId,Validators.required],
      name: ['', Validators.required],
    });

    this.franchisesService.GetbyId(this.franchiseId).subscribe(
      (group) => {
        // Llena el formulario con los datos del ambiente
        this.franchiseForm.patchValue({
          name: group.name,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
  }

  onSubmit(): void {
    if (this.franchiseForm.valid) {
      this.franchisesService.Update(this.franchiseId,this.franchiseForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/franchises']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/franchises']);
  }
}

