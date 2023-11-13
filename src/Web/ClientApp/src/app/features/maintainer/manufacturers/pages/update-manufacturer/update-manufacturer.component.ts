import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { manufacturerVM } from 'src/app/core/models/manufacturer.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-manufacturer.component.html',
  styleUrls: ['./update-manufacturer.component.scss']
})
export class UpdateManufacturerComponent {
  manufacturerId!:string | null;
  manufacturerForm! : FormGroup;

  constructor(private manufacturersService: ManufacturersService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.manufacturerId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.manufacturerForm = this.formBuilder.group({
      id:[this.manufacturerId,Validators.required],
      name: ['', Validators.required],
    });

    this.manufacturersService.GetbyId(this.manufacturerId).subscribe(
      (manufacturer) => {
        // Llena el formulario con los datos del ambiente
        this.manufacturerForm.patchValue({
          name: manufacturer.name,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de el fabricante', error);
      }
    );
  }

  onSubmit(): void {
    if (this.manufacturerForm.valid) {
      this.manufacturersService.Update(this.manufacturerId,this.manufacturerForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/manufacturers']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/manufacturers']);
  }
}
