import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommuneByRegion, RegionDto } from '@app/core/models';
import { CommuneService, NotificationService, RegionService, StoreService } from '@app/core/services';
import { ButtonComponent } from '@app/shared';
import { NgSelectModule } from '@ng-select/ng-select';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-add-store',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule, ButtonComponent, NgSelectModule
  ],
  templateUrl: './add-store.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddStoreComponent implements OnInit{

  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);
  private regionService = inject(RegionService);
  private communeService = inject(CommuneService);

  public storeForm!: FormGroup;
  public regions = signal<RegionDto[]>([]);
  public communes = signal<CommuneByRegion[]>([]);

  public ngOnInit(): void {
    this.storeForm = this.formBuilder.group({
      storeName: ['', Validators.required],
      storeWebSite: ['', Validators.required],
      storeAddress: this.formBuilder.array([]),
    });

    this.addStoreAddress();
    this.loadRegions();
  }

  public loadRegions() {
    this.regionService.getRegions().subscribe({
      next: (response) => {

        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.regions.set(response.data.filter(region => region.isActive));
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onChangeRegion(index:number): void {

    const regionId = this.storeAddress.at(index).get('regionId')?.value;

    if (!regionId)
      return;

    this.communeService.getCommunesByRegionId(regionId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.communes.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  

  public addStoreAddress(): void {
    const storeAddress = this.storeForm.get('storeAddress') as FormArray;
    storeAddress.push(this.formBuilder.group({
      storeId: [''],
      regionId: [null, Validators.required],
      communeId: [null, Validators.required],
      street: ['', Validators.required],
      zipCode: [''],
    }));
  }

  public get storeAddress() {
    return this.storeForm.controls["storeAddress"] as FormArray;
  }

  public removeStoreAddress(index: number): void {
    this.storeAddress.removeAt(index);
  }

  public onSubmit(): void {
    if (this.storeForm.invalid) {
      return;
    }

    this.storeService.createStore(this.storeForm.value).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        var storeId = response.data;
        const saveObservables = [];

        for (let index = 0; index < this.storeAddress.length; index++) {
          const storeAddress = this.storeAddress.at(index).value;

          storeAddress.storeId = storeId;

          saveObservables.push(this.storeService.createStoreAddress(storeId, storeAddress));
        }

        forkJoin(saveObservables).subscribe({
          next: (responses) => {
            responses.forEach((response) => {
              if (!response.success) {
                this.notificationService.showError("Error", response.message);
                return;
              }
            });
          },
          error: () => {
            this.notificationService.showDefaultError();
          }
        });

        this.notificationService.showSuccess("Exito", response.message);
        this.router.navigate(['/maintainer/stores']);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/stores']);
  }

}
