import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { GroupsService } from 'src/app/core/services/groups.service';
import { groupDto } from 'src/app/core/models/group.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './add-category.component.html',
  styleUrls: ['./add-category.component.scss']
})
export class AddCategoryComponent {
  categoryForm! : FormGroup;
  groups = signal<groupDto[]>([]);

  constructor(private formbuilder: FormBuilder, private categoriesService: CategoriesService, private groupsService : GroupsService, private router:Router) {
    this.createForm();
    this.loadGroups();
  }

  createForm() {
    this.categoryForm = this.formbuilder.group({
      name: ['',Validators.required],
      groupId:['',Validators.required]
    });
  }

  loadGroups(){
    this.groupsService.GetAll().subscribe(
      (groups) => {
        this.groups.set(groups.filter(group => group.active));
      }
    )
  }

  onSubmit():void{
    if(this.categoryForm.valid){
      this.categoriesService.Create(this.categoryForm.value).subscribe(() => {
        this.router.navigate(['maintainer/categories']);
      }, error => {
        // Manejar el error
      });
    }
  }

  onCancel():void{
    this.router.navigate(['/maintainer/categories']);
  }
}