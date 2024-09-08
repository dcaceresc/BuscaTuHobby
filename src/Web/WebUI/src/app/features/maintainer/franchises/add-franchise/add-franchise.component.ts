import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FranchiseService } from '../../../../core/services/maintainer/franchise.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-franchise',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule
  ],
  templateUrl: './add-franchise.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddFranchiseComponent implements OnInit { 

  private franchiseService = inject(FranchiseService);
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);

  public franchiseForm!: FormGroup;

  public ngOnInit(){
    this.franchiseForm = this.formBuilder.group({
      franchiseName: ['',Validators.required],
    });
  }

  public onSubmit(){

    if(this.franchiseForm.invalid){
      return;
    }

    this.franchiseService.addFranchise(this.franchiseForm.value).subscribe({

    });

  }

  public onCancel(){
    this.router.navigate(['/maintainer/franchises']);
  }
}
