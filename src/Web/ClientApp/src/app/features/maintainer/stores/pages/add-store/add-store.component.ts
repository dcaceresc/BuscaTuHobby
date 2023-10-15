import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { StoresService } from 'src/app/core/services/stores.service';
import { faStar } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,FontAwesomeModule],
  templateUrl: './add-store.component.html',
  styleUrls: ['./add-store.component.scss']
})
export class AddStoreComponent {
  storeForm! : FormGroup;
  faStar = faStar;

  constructor(private formbuilder: FormBuilder, private storesService: StoresService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.storeForm = this.formbuilder.group({
      name: ['',Validators.required],
      address : ['',Validators.required],
      webSite: ['',Validators.required]
    });
  }

  onSubmit():void{
    if(this.storeForm.valid){
      this.storesService.Create(this.storeForm.value).subscribe(() => {
        this.router.navigate(['maintainer/stores']);
      }, error => {
        console.log(error);
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/stores']);
  }
}
