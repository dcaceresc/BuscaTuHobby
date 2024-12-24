import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FranchiseDto } from '@app/core/models';
import { FranchiseService, NotificationService, SerieService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
    selector: 'app-add-serie',
    imports: [
        CommonModule, ReactiveFormsModule, NgSelectModule
    ],
    templateUrl: './add-serie.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddSerieComponent implements OnInit { 
  
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private serieService = inject(SerieService);
  private franchiseService = inject(FranchiseService);
  private notificationService = inject(NotificationService);

  public serieForm!: FormGroup;
  public franchises = signal<FranchiseDto[]>([]);

  public ngOnInit(): void {
    this.serieForm = this.formBuilder.group({
      serieName: ['', Validators.required],
      franchiseId: ['', Validators.required]
    });

    this.loadFranchises();
  }

  public loadFranchises() {
    this.franchiseService.getFranchises().subscribe({
      next: (response) => {
        
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.franchises.set(response.data.filter(x => x.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public onSubmit(): void {
    if (this.serieForm.invalid) {
      return;
    }

    this.serieService.createSerie(this.serieForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/series']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/series']);
  }
}
