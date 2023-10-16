import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ScalesService } from 'src/app/core/services/scales.service';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-scale.component.html',
  styleUrls: ['./add-scale.component.scss']
})
export class AddScaleComponent {
  scaleForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private scalesService: ScalesService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.scaleForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.scaleForm.valid){
      this.scalesService.Create(this.scaleForm.value).subscribe(() => {
        this.router.navigate(['maintainer/scales']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/scales']);
  }
}
