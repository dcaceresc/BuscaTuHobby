import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RegionDto } from '@app/core/models';
import { CommuneService, NotificationService, RegionService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-update-commune',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,NgSelectModule
  ],
  templateUrl: './update-commune.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateCommuneComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private communeService = inject(CommuneService);
  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);

  public communeId! : string | null;
  public communeForm!: FormGroup;
  public regions = signal<RegionDto[]>([]);

  public ngOnInit(): void {
    this.communeId = this.route.snapshot.paramMap.get('id');
    this.communeForm = this.formBuilder.group({
      communeId: [this.communeId],
      communeName: ['',Validators.required],
      regionId: ['',Validators.required]
    });
    this.loadRegions();
    this.loadCommune();
  }

  public loadRegions() {
    this.regionService.getRegions().subscribe({
      next: (response) => {
        this.regions.set(response.data.filter(region => region.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public loadCommune() {
    if (this.communeId) {
      this.communeService.getCommuneById(this.communeId).subscribe({
        next: (response) => {
          this.communeForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public onSubmit() {
    if (this.communeForm.invalid) {
      return;
    }

    this.communeService.updateCommune(this.communeId,this.communeForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }
        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/communes']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel() {
    this.router.navigate(['/maintainer/communes']);
  }

}
