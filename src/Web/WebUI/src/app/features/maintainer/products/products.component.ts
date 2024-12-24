import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductDto } from '@app/core/models';
import { FaIconService, NotificationService, ProductService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-products',
    imports: [
        CommonModule, ButtonComponent, TableComponent, RouterLink
    ],
    templateUrl: './products.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsComponent implements OnInit {

  private productService = inject(ProductService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<ProductDto[]>([]);
  public actions: any[] = [];

  public ngOnInit() {
    this.columns = [
      { name: '#', key: 'productId' },
      { name: 'Nombre', key: 'productName' },
      { name: 'Escala', key: 'scaleName' },
      { name: 'Fabricante', key: 'manufacturerName' },
      { name: 'Franquicia', key: 'franchiseName' },
      { name: 'Serie', key: 'serieName' },
      { name: 'Categorias', key: 'categories' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]

    this.loadProducts();
  }


  public loadProducts() {
    this.productService.getProducts().subscribe({
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

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/products/update', id]);
  }

  public onToggle(id: string) {
    this.productService.toggleProduct(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.loadProducts();
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
