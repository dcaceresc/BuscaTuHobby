import { Routes } from "@angular/router";

export const routes : Routes =[
  { path: 'series', loadChildren: () => import("./series/series.routes").then(m => m.routes) },
  { path: 'stores', loadChildren: () => import("./stores/stores.routes").then(m => m.routes) },
  { path: 'scales', loadChildren: () => import("./scales/scales.routes").then(m => m.routes) },
  { path: 'categories', loadChildren: () => import("./categories/categories.routes").then(m => m.routes) },
  { path: 'manufacturers', loadChildren: () => import("./manufacturers/manufacturers.routes").then(m => m.routes) },
  { path: 'products', loadChildren: () => import("./products/products.routes").then(m => m.routes) },
];
