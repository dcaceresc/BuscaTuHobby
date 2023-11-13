import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { storeVM } from 'src/app/core/models/store.model';
import { ActivatedRoute, Router } from '@angular/router';
import { StoresService } from 'src/app/core/services/stores.service';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-store.component.html',
  styleUrls: ['./update-store.component.scss']
})
export class UpdateStoreComponent {
  storeId!:string | null;
  storeForm! : FormGroup;

  constructor(private storesService: StoresService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.storeId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.storeForm = this.formBuilder.group({
      id:[this.storeId,Validators.required],
      name: ['', Validators.required],
      address : ['',Validators.required],
      webSite : ['',Validators.required]
    });

    this.storesService.GetbyId(this.storeId).subscribe(
      (store) => {
        // Llena el formulario con los datos del ambiente
        this.storeForm.patchValue({
          name: store.name,
          address : store.address,
          webSite : store.webSite,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la tienda', error);
      }
    );
  }

  onSubmit(): void {
    if (this.storeForm.valid) {
      this.storesService.Update(this.storeId,this.storeForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/stores']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }

  onCancel(): void {
    this.router.navigate(['/maintainer/stores']);
  }
}
