import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { CommuneService } from '../../../../core/services/maintainer/commune.service';
import { RegionService } from '../../../../core/services/maintainer/region.service';
import { RegionDto } from '../../../../core/models/maintainer/region.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-add-commune',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule, NgSelectModule
  ],
  templateUrl: './add-commune.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddCommuneComponent implements OnInit {

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private communeService = inject(CommuneService);
  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);

  public communeForm!: FormGroup;
  public regions = signal<RegionDto[]>([]);

  public ngOnInit() : void {
    this.communeForm = this.formBuilder.group({
      communeName: ['',Validators.required],
      regionId: [null,Validators.required]
    });

    this.loadRegions();
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

  public onSubmit() {
    if (this.communeForm.invalid) {
      return;
    }

    this.communeService.createCommune(this.communeForm.value).subscribe({
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
