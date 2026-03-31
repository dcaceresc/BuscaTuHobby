import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./posts.component').then(m => m.PostsComponent)
  },
  {
    path: 'add',
    loadComponent: () => import('./add-edit-post/add-edit-post.component').then(m => m.AddEditPostComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./add-edit-post/add-edit-post.component').then(m => m.AddEditPostComponent)
  }
];
