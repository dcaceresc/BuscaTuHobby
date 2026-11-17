import { Routes } from "@angular/router";
import { AdminGuard } from "../../../core/guards/admin.guard";

// La API publica api/maintainer/ingestion con la política SuperAdmin y Administrator, así que la pantalla usa la
// misma guardia que el resto del mantenedor: un usuario sin ese rol vería las llamadas rechazadas.
export const routes : Routes = [
    {path: '', canActivate: [AdminGuard], loadComponent: () => import('./ingestion.component').then(m => m.IngestionComponent)},
];
