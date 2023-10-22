import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GroupsService } from 'src/app/core/services/groups.service';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './add-group.component.html',
  styleUrls: ['./add-group.component.scss']
})
export class AddGroupComponent {
  groupForm! : FormGroup;

  constructor(private formbuilder: FormBuilder, private groupsService: GroupsService, private router:Router) {
    this.createForm();
  }

  createForm() {
    this.groupForm = this.formbuilder.group({
      name: ['',Validators.required],
    });
  }

  onSubmit():void{
    if(this.groupForm.valid){
      this.groupsService.Create(this.groupForm.value).subscribe(() => {
        this.router.navigate(['maintainer/groups']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/groups']);
  }
}
