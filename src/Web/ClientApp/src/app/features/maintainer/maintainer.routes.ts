import { Routes } from "@angular/router";

export const routes : Routes =[
  { path: 'universes', loadChildren: () => import("./universes/universes.routes").then(m => m.routes) },
  { path: 'series', loadChildren: () => import("./series/series.routes").then(m => m.routes) },
  { path: 'stores', loadChildren: () => import("./stores/stores.routes").then(m => m.routes) },
];
