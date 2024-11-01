import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommuneByRegion, RegionDto } from '@app/core/models';
import { CommuneService, NotificationService, RegionService, StoreService } from '@app/core/services';
import { ButtonComponent } from '@app/shared';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-update-store',
  standalone: true,
  imports: [
    CommonModule,ReactiveFormsModule,ButtonComponent,NgSelectModule
  ],
  templateUrl: './update-store.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateStoreComponent implements OnInit{ 

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private storeService = inject(StoreService);
  private notificationService = inject(NotificationService);
  private regionService = inject(RegionService);
  private communeService = inject(CommuneService);

  public storeForm!: FormGroup;
  public storeId!: string | null;
  public regions = signal<RegionDto[]>([]);
  public communes = signal<CommuneByRegion[]>([]);

  public ngOnInit(): void {

    this.storeId = this.route.snapshot.paramMap.get('id');

    this.storeForm = this.formBuilder.group({
      storeId: [this.storeId],
      storeName: ['', Validators.required],
      storeWebSite: ['', Validators.required],
      storeAddress: this.formBuilder.array([]),
    });

    this.storeService.getStoreById(this.storeId).subscribe({
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


  public addStoreAddress(): void {
    const storeAddress = this.storeForm.get('storeAddress') as FormArray;
    storeAddress.push(this.formBuilder.group({
      storeAddressId: [null],
      storeId: [''],
      regionId: [null, Validators.required],
      communeId: [null, Validators.required],
      street: ['', Validators.required],
      zipCode: [''],
      deleted: [false]
    }));
  }

  public get storeAddress() {
    return this.storeForm.controls["storeAddress"] as FormArray;
  }

  public removeStoreAddress(index: number): void {
    const storeAddress = this.storeAddress.at(index);
    storeAddress.get('deleted')?.setValue(true);

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

  public onSubmit(){
    if(this.storeForm.invalid){
      return;
    }

    this.storeService.updateStore(this.storeId, this.storeForm.value).subscribe({
      next: (response) => {

        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        var storeAddress = this.storeForm.get('storeAddress') as FormArray;

        storeAddress.controls.forEach(control => {

          control.get('storeId')?.setValue(this.storeId);

          if(control.get('deleted')?.value){
            this.storeService.deleteStoreAddress(this.storeId, control.get('storeAddressId')?.value).subscribe({
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

            this.storeService.createStoreAddress(this.storeId, control.value).subscribe({
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

            this.storeService.updateStoreAddress(this.storeId, control.get('storeAddressId')?.value, control.value).subscribe({
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
  }

  public onCancel(){
    this.router.navigate(['/maintainer/stores']);
  }
}
