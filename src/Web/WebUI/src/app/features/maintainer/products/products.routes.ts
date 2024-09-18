import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./products.component').then(m => m.ProductsComponent)},
    {path: 'add', loadComponent: () => import('./add-product/add-product.component').then(m => m.AddProductComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-product/update-product.component').then(m => m.UpdateProductComponent)},
];