import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { TableComponent } from '../../../shared/components/table/table.component';
import { CategoryService } from '../../../core/services/maintainer/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CategoryDto } from '../../../core/models/maintainer/category.model';
import { FaIconService } from '../../../core/services/fa-icon.service';

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
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<CategoryDto[]>([]);
  public actions : any[] = [];
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'categoryId' },
      { name: 'Nombre', key: 'categoryName' },
      { name: 'Grupo', key: 'groupName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]
    
    this.loadCategories();
    
  }

  public loadCategories(): void {
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

  public onEdit(categoryId: string): void {
    this.router.navigate(['maintainer/categories/update', categoryId]);
  }

  public onToggle(categoryId: string): void {
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

  public onAction(event: { id: string, actionKey: string }) {
    switch (event.actionKey) {
      case 'edit':
        this.onEdit(event.id);
        break;
      case 'toggle':
        this.onToggle(event.id);
        break;
    }
  }



}
