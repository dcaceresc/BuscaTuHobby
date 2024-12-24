import { ChangeDetectionStrategy, Component, inject, OnInit, signal, input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RegionDto } from '@app/core/models';
import { CommuneService, NotificationService, RegionService } from '@app/core/services';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
    selector: 'app-add-edit-commune',
    imports: [ReactiveFormsModule, NgSelectModule],
    templateUrl: './add-edit-commune.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditCommuneComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private communeService = inject(CommuneService);
  private regionService = inject(RegionService);
  private notificationService = inject(NotificationService);

  readonly communeId = input.required<string | null>({ alias: "id" });
  public isEditMode : boolean = false;
  public communeForm!: FormGroup;
  public regions = signal<RegionDto[]>([]);

  public ngOnInit() : void {
    this.isEditMode = !!this.communeId();
    this.createForm();
    this.loadRegions();

    if(this.isEditMode){
      this.communeService.getCommuneById(this.communeId()).subscribe({
        next: (response) => {

          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }

          this.communeForm.patchValue(response.data);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm() : void{
    if(this.isEditMode){
      this.communeForm = this.formBuilder.group({
        communeId: [this.communeId(),Validators.required],
        communeName: ['',Validators.required],
        regionId: [null,Validators.required]
      });
    }else{
      this.communeForm = this.formBuilder.group({
        communeName: ['',Validators.required],
        regionId: [null,Validators.required]
      });
    }
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
      this.notificationService.showInvalidFormError();
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
