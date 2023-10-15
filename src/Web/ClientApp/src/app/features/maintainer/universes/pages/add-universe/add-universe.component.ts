import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UniversesService } from 'src/app/core/services/universes.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-universe.component.html',
  styleUrls: ['./add-universe.component.scss']
})
export class AddUniverseComponent {
  universeForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private universeService: UniversesService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.universeForm = this.formbuilder.group({
      name: ['',Validators.required],
      acronym : ['',Validators.required]
    });
  }

  onSubmit():void{
    if(this.universeForm.valid){
      this.universeService.Create(this.universeForm.value).subscribe(() => {
        this.router.navigate(['maintainer/universes']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/universes']);
  }
}
