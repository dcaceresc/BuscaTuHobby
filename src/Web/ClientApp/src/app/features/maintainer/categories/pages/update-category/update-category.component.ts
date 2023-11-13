import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { categoryVM } from 'src/app/core/models/category.model';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoriesService } from 'src/app/core/services/categories.service';
import { GroupsService } from 'src/app/core/services/groups.service';
import { NgSelectModule } from '@ng-select/ng-select';
import { groupDto } from 'src/app/core/models/group.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule,NgSelectModule],
  templateUrl: './update-category.component.html',
  styleUrls: ['./update-category.component.scss']
})
export class UpdateCategoryComponent {
  categoryId!:string | null;
  categoryForm! : FormGroup;
  groups = signal<groupDto[]>([]);

  constructor(private categoriesService: CategoriesService,
    private router: Router,
    private formBuilder: FormBuilder,
    private groupsService : GroupsService,
    private route: ActivatedRoute,) {
      this.categoryId = this.route.snapshot.paramMap.get('id');
      this.loadGroups();
  }

  ngOnInit(): void {
    this.categoryForm = this.formBuilder.group({
      id:[this.categoryId,Validators.required],
      name: ['', Validators.required],
      groupId : ['', Validators.required],
    });

    this.categoriesService.GetbyId(this.categoryId).subscribe(
      (category) => {
        // Llena el formulario con los datos del ambiente
        this.categoryForm.patchValue({
          name: category.name,
          groupId: category.groupId,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
  }

  loadGroups(){
    this.groupsService.GetAll().subscribe(
      (groups) => {
        this.groups.set(groups.filter(group => group.active));
      }
    )
  }

  onSubmit(): void {
    if (this.categoryForm.valid) {
      this.categoriesService.Update(this.categoryId,this.categoryForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/categories']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/categories']);
  }
}
