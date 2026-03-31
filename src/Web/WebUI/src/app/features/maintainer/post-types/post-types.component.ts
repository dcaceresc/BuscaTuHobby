import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { PostTypeDto } from '@app/core/models';
import { NotificationService, PostTypeService } from '@app/core/services';
import { SearchComponent, TableComponent } from '@app/shared';

@Component({
  selector: 'app-post-types',
  imports: [RouterLink, TableComponent, SearchComponent],
  templateUrl: './post-types.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostTypesComponent implements OnInit {

  private router = inject(Router);
  private postTypeService = inject(PostTypeService);
  private notificationService = inject(NotificationService);

  public columns: any[] = [];
  public data = signal<PostTypeDto[]>([]);
  public actions: any[] = [];
  public searchTerm = signal('');

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'postTypeId' },
      { name: 'Nombre', key: 'postTypeName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle' },
    ];

    this.loadPostTypes();
  }

  public loadPostTypes(): void {
    this.postTypeService.getPostTypes().subscribe({
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

  public onEdit(postTypeId: string): void {
    this.router.navigate(['maintainer/post-types/edit', postTypeId]);
  }

  public onToggle(postTypeId: string): void {
    this.postTypeService.togglePostType(postTypeId.toString()).subscribe({
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

  public onSearch(term: string): void {
    this.searchTerm.set(term);
  }
}
