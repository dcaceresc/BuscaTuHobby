import { Routes } from "@angular/router";

export const routes : Routes =[
    
    { path: 'categories', loadChildren: () => import("./categories/categories.routes").then(m => m.routes) },
    { path: 'groups', loadChildren: () => import("./groups/groups.routes").then(m => m.routes) },
    { path: 'franchises', loadChildren: () => import("./franchises/franchises.routes").then(m => m.routes) },
    { path: 'manufacturers', loadChildren: () => import('./manufacturers/manufacturers.routes').then(m => m.routes)},
    { path: 'scales', loadChildren: () => import('./scales/scales.routes').then(m => m.routes) },
    { path: 'series', loadChildren: () => import('./series/series.routes').then(m => m.routes) },
    {path: 'stores', loadChildren: () => import('./stores/stores.routes').then(m => m.routes)},
    {path: 'products', loadChildren: () => import('./products/products.routes').then(m => m.routes)},
    {path: 'configurations', loadChildren: () => import('./configurations/configurations.routes').then(m => m.routes)},
    {path: 'inventories', loadChildren: () => import('./inventories/inventories.routes').then(m => m.routes)},
    {path: 'regions', loadChildren: () => import('./regions/regions.routes').then(m => m.routes)},
    {path: 'communes', loadChildren: () => import('./communes/communes.routes').then(m => m.routes)},
];