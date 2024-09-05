import { Routes } from "@angular/router";

export const routes : Routes =[
    {
        path : '', 
        loadComponent : () => import('./categories.component').then(m => m.CategoriesComponent)
    },
    {
        path : 'add', 
        loadComponent : () => import('./add-category/add-category.component').then(m => m.AddCategoryComponent)
    },
    {
        path : 'update/:id', 
        loadComponent : () => import('./update-category/update-category.component').then(m => m.UpdateCategoryComponent)
    }
];