import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FranchisesService } from 'src/app/core/services/franchises.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-franchise.component.html',
  styleUrls: ['./add-franchise.component.scss']
})
export class AddFranchiseComponent {
  franchiseForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private franchisesService: FranchisesService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.franchiseForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.franchiseForm.valid){
      this.franchisesService.Create(this.franchiseForm.value).subscribe(() => {
        this.router.navigate(['maintainer/franchises']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/franchises']);
  }
}
