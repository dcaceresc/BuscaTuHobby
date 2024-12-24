import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FranchiseDto } from '@app/core/models';
import { FranchiseService, NotificationService, SerieService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
    selector: 'app-update-serie',
    imports: [
        CommonModule, ReactiveFormsModule, NgSelectModule
    ],
    templateUrl: './update-serie.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpdateSerieComponent implements OnInit{ 

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private serieService = inject(SerieService);
  private franchiseService = inject(FranchiseService);
  private notificationService = inject(NotificationService);

  public serieId!: string | null;
  public serieForm!: FormGroup;
  public franchises = signal<FranchiseDto[]>([]);

  public ngOnInit(): void {
    this.serieId = this.route.snapshot.paramMap.get('id');
    
    this.serieForm = this.formBuilder.group({
      serieId: [this.serieId],
      serieName: ['', Validators.required],
      franchiseId: ['', Validators.required]
    });

    this.serieService.getSerieById(this.serieId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.serieForm.patchValue(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
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

    this.serieService.updateSerie(this.serieId,this.serieForm.value).subscribe({
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
