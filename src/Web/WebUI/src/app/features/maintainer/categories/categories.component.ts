import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CategoryDto } from '@app/core/models';
import { CategoryService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-categories',
    imports: [RouterLink, ButtonComponent, TableComponent],
    templateUrl: './categories.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoriesComponent implements OnInit{
  
  private router = inject(Router);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);

  public columns :any[] = [];
  public data = signal<CategoryDto[]>([]);
  public actions : any[] = [];
  
  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'categoryId' },
      { name: 'Nombre', key: 'categoryName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi bi-toggle-on', actionKey: 'toggle'},
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
    this.router.navigate(['maintainer/categories/edit', categoryId]);
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
