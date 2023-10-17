import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-manufacturer.component.html',
  styleUrls: ['./add-manufacturer.component.scss']
})
export class AddManufacturerComponent {
  manufacturerForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private manufacturersService: ManufacturersService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.manufacturerForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.manufacturerForm.valid){
      this.manufacturersService.Create(this.manufacturerForm.value).subscribe(() => {
        this.router.navigate(['maintainer/manufacturers']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/manufacturers']);
  }
}
