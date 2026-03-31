import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { PostDto } from '@app/core/models';
import { NotificationService, PostService } from '@app/core/services';
import { SearchComponent, TableComponent } from '@app/shared';

@Component({
  selector: 'app-posts',
  imports: [RouterLink, TableComponent, SearchComponent],
  templateUrl: './posts.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostsComponent implements OnInit {

  private router = inject(Router);
  private postService = inject(PostService);
  private notificationService = inject(NotificationService);

  public columns: any[] = [];
  public data = signal<PostDto[]>([]);
  public actions: any[] = [];
  public searchTerm = signal('');

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'postId' },
      { name: 'Titulo', key: 'postTitle' },
      { name: 'Tipo', key: 'postTypeName' },
      { name: 'Categorias', key: 'categories' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi-toggle-on', actionKey: 'toggle' },
    ];

    this.loadPosts();
  }

  public loadPosts(): void {
    this.postService.getPosts().subscribe({
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

  public onEdit(postId: string): void {
    this.router.navigate(['maintainer/posts/edit', postId]);
  }

  public onToggle(postId: string): void {
    this.postService.togglePost(postId.toString()).subscribe({
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
