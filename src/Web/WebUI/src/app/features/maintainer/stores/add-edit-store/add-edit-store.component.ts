import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal, input } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommuneByRegion, RegionDto } from '@app/core/models';
import { CommuneService, NotificationService, RegionService, StoreService } from '@app/core/services';
import { CustomInputComponent } from '@app/shared/components/custom-input/custom-input.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-add-edit-store',
    imports: [ReactiveFormsModule, NgSelectModule, NgClass, CustomInputComponent],
    templateUrl: './add-edit-store.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditStoreComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);
  private regionService = inject(RegionService);
  private communeService = inject(CommuneService);

  readonly storeId = input.required<string | null>({ alias: "id" });
  public isEditMode: boolean = false;
  public storeForm!: FormGroup;
  public regions = signal<RegionDto[]>([]);
  public communes = signal<CommuneByRegion[]>([]);

  public ngOnInit(): void {
    this.isEditMode = !!this.storeId();
    this.createForm();
    this.addStoreAddress();
    this.loadRegions();

    if(this.isEditMode){
      this.storeService.getStoreById(this.storeId()).subscribe({
        next: (response) => {
  
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          this.storeForm.patchValue(response.data);
          response.data.storeAddress.forEach((storeAddress: any, index: number) => {
            const storeAddressForm = this.formBuilder.group({
              storeAddressId: [storeAddress.storeAddressId],
              storeId: [storeAddress.storeId],
              regionId: [storeAddress.regionId, Validators.required],
              communeId: [storeAddress.communeId, Validators.required],
              street: [storeAddress.street, Validators.required],
              zipCode: [storeAddress.zipCode],
              deleted: [false]
            });
      
            this.storeAddress.push(storeAddressForm);
      
            // Disparar evento de cambio de región para cargar las comunas
            this.onChangeRegion(index);
          });
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.storeForm = this.formBuilder.group({
        storeId: [this.storeId(),Validators.required],
        storeName: ['', Validators.required],
        storeIcon : ['', Validators.required],
        storeOrder: ['', Validators.required],
        storeSlug: ['', Validators.required],
        storeWebSite: ['', Validators.required],
        storeAddress: this.formBuilder.array([]),
      });
    }else{
      this.storeForm = this.formBuilder.group({
        storeName: ['', Validators.required],
        storeIcon : ['', Validators.required],
        storeOrder: ['', Validators.required],
        storeSlug: ['', Validators.required],
        storeWebSite: ['', Validators.required],
        storeAddress: this.formBuilder.array([]),
      });
    }
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
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.storeService.updateStore(this.storeId(), this.storeForm.value).subscribe({
        next: (response) => {
  
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          var storeAddress = this.storeForm.get('storeAddress') as FormArray;
  
          storeAddress.controls.forEach(control => {
  
            const storeId = this.storeId();
            control.get('storeId')?.setValue(storeId);
  
            if(control.get('deleted')?.value){
              this.storeService.deleteStoreAddress(storeId, control.get('storeAddressId')?.value).subscribe({
                next: (response) => {
                  if(!response.success){
                    this.notificationService.showError("Error", response.message);
                    return;
                  }
                },
                error: () => {
                  this.notificationService.showDefaultError();
                }
              });
            }else if(control.get('storeAddressId')?.value == null){
  
              this.storeService.createStoreAddress(storeId, control.value).subscribe({
                next: (response) => {
                  if(!response.success){
                    this.notificationService.showError("Error", response.message);
                    return;
                  }
                },
                error: () => {
                  this.notificationService.showDefaultError();
                }
              });
            }else{
  
              this.storeService.updateStoreAddress(storeId, control.get('storeAddressId')?.value, control.value).subscribe({
                next: (response) => {
                  if(!response.success){
                    this.notificationService.showError("Error", response.message);
                    return;
                  }
                },
                error: () => {
                  this.notificationService.showDefaultError();
                }
              });
            }
          });
  
  
          this.notificationService.showSuccess("Success", response.message);
          this.router.navigate(['/maintainer/stores']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }else{
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

    
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/stores']);
  }
}
