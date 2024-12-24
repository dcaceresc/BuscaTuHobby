import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit, input } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MenuService, NotificationService } from '@app/core/services';
import { ButtonComponent } from '@app/shared';

@Component({
    selector: 'app-add-edit-menu',
    imports: [ReactiveFormsModule, ButtonComponent],
    templateUrl: './add-edit-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddEditMenuComponent implements OnInit {
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  private menuService = inject(MenuService);
  private notificationService = inject(NotificationService);
  private changeDetectorRef = inject(ChangeDetectorRef);

  readonly menuId = input.required<string | null>({ alias: "id" });
  public isEditMode: boolean = false;
  public menuForm!: FormGroup;

  public ngOnInit(): void {
    this.isEditMode = !!this.menuId();
    this.createForm();

    if(this.isEditMode){
      this.getMenu();
    }
  }

  public createForm(): void {
    if(this.isEditMode){
      this.menuForm = this.formBuilder.group({
        menuId: [this.menuId(), Validators.required],
        menuName: ['', Validators.required],
        menuOrder: ['', Validators.required],
        subMenus: this.formBuilder.array([]),
      });
    }else{
      this.menuForm = this.formBuilder.group({
        menuName: ['', Validators.required],
        menuOrder: ['', Validators.required],
        subMenus: this.formBuilder.array([]),
      });
    }
  }

  public getMenu(){
    this.menuService.getMenuById(this.menuId()).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

        this.menuForm.patchValue({
          menuId: response.data.menuId,
          menuName: response.data.menuName,
          menuOrder: response.data.menuOrder,
        })

        response.data.subMenus.forEach((subMenu: any) => {
          const subMenuGroup = this.formBuilder.group({
            subMenuId: subMenu.subMenuId,
            subMenuName: subMenu.subMenuName,
            subMenuOrder: subMenu.subMenuOrder,
            isActive: subMenu.isActive,
          });

          this.subMenus.push(subMenuGroup);
        });
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public get subMenus(): FormArray {
    return this.menuForm.controls['subMenus'] as FormArray;
  }


  public addSubMenu(): void {

    if(this.isEditMode){
      const subMenu = this.formBuilder.group({
        subMenuName: ['', Validators.required],
        subMenuOrder: ['', Validators.required],
        isActive: [null],
      });
      this.subMenus.push(subMenu);
    }else{
      const subMenu = this.formBuilder.group({
        subMenuName: ['', Validators.required],
        subMenuOrder: ['', Validators.required],
      });
      this.subMenus.push(subMenu);
      
    }

    
  }

  public removeSubMenu(index: number): void {
    this.subMenus.removeAt(index);
  }

  public toggleSubMenu(index: number): void {
    const subMenu = this.subMenus.at(index);
    
    this.menuService.toggleSubMenu(this.menuId(),subMenu.value.subMenuId).subscribe({
      next: (response) => {
        if(!response.success){
          this.notificationService.showError("Error", response.message);
          return;
        }

       
        this.notificationService.showSuccess("Exito", response.message);

        const isActive = subMenu.get('isActive')?.value;
        subMenu.get('isActive')?.setValue(!isActive);

         // Trigger change detection
         this.changeDetectorRef.detectChanges();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onSubmit(): void {
    if (this.menuForm.invalid) {
      this.notificationService.showInvalidFormError();
      return;
    }

    if(this.isEditMode){
      this.menuService.updateMenu(this.menuId(),this.menuForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }

          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/menus']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }else{
      this.menuService.createMenu(this.menuForm.value).subscribe({
        next: (response) => {
          if(!response.success){
            this.notificationService.showError("Error", response.message);
            return;
          }
  
          this.notificationService.showSuccess("Exito", response.message);
          this.router.navigate(['/maintainer/menus']);
        },
        error: () => {
          this.notificationService.showDefaultError();
        }
      });
    }
  }

  public onCancel(): void {
    this.router.navigate(['/maintainer/menus']);
  }
}
