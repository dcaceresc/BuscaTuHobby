import { Routes } from "@angular/router";

export const routes : Routes =[
    {
        path : '', 
        loadComponent : () => import('./categories.component').then(m => m.CategoriesComponent)
    },
    {
        path : 'add', 
        loadComponent : () => import('./add-edit-category/add-edit-category.component').then(m => m.AddEditCategoryComponent)
    },
    {
        path : 'edit/:id', 
        loadComponent : () => import('./add-edit-category/add-edit-category.component').then(m => m.AddEditCategoryComponent)
    }
];