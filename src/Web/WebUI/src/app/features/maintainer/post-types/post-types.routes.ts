import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./post-types.component').then(m => m.PostTypesComponent)
  },
  {
    path: 'add',
    loadComponent: () => import('./add-edit-post-type/add-edit-post-type.component').then(m => m.AddEditPostTypeComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./add-edit-post-type/add-edit-post-type.component').then(m => m.AddEditPostTypeComponent)
  }
];
