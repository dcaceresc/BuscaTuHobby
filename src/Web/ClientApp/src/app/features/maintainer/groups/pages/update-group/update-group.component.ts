import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupsService } from 'src/app/core/services/groups.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { groupVM } from 'src/app/core/models/group.model';

@Component({
  standalone: true,
  imports: [CommonModule,ReactiveFormsModule],
  templateUrl: './update-group.component.html',
  styleUrls: ['./update-group.component.scss']
})
export class UpdateGroupComponent {
  groupId!:string | null;
  groupForm! : FormGroup;

  constructor(private groupsService: GroupsService,
    private router: Router,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,) {
      this.groupId = this.route.snapshot.paramMap.get('id');
  }

  ngOnInit(): void {
    this.groupForm = this.formBuilder.group({
      id:[this.groupId,Validators.required],
      name: ['', Validators.required],
    });

    this.groupsService.GetbyId(this.groupId).subscribe(
      (group) => {
        // Llena el formulario con los datos del ambiente
        this.groupForm.patchValue({
          name: group.name,
          // Completa aquí los demás campos del formulario con los datos correspondientes
        });
      },
      (error) => {
        console.error('Error al cargar los datos de la escala', error);
      }
    );
  }

  onSubmit(): void {
    if (this.groupForm.valid) {
      this.groupsService.Update(this.groupId,this.groupForm.value).subscribe(
        () => {
          this.router.navigate(['/maintainer/groups']);
        },
        error => {
          // Manejar el error
        }
      );
    }
  }


  onCancel(): void {
    this.router.navigate(['/maintainer/groups']);
  }
}

