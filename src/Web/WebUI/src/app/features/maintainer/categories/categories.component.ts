import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { CategoryService } from '../../../core/services/maintainer/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CategoryDto } from '../../../core/models/maintainer/category.model';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [
    RouterLink,ButtonComponent,TableComponent
  ],
  templateUrl : './categories.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriesComponent implements OnInit{
  
  private router = inject(Router);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);

  public columns :any[] = [];
  public data = signal<CategoryDto[]>([]);
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'categoryId' },
      { name: 'Nombre', key: 'categoryName' },
      { name: 'Grupo', key: 'groupName' },
      { name: 'Acciones', key: 'isActive' },
    ];
    this.categoryService.getCategories().subscribe({
      next: (response) => {
        
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
    
  }

  public onEdit(categoryId: number): void {
    this.router.navigate(['maintainer/categories/update', categoryId]);
  }

  public onToggle(categoryId: number): void {
    this.categoryService.toggleCategory(categoryId.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.ngOnInit();
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }



}
