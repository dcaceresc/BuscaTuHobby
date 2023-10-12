import { Routes } from "@angular/router";
import { ListSeriesComponent } from "./pages/list-series/list-series.component";
import { AddSeriesComponent } from "./pages/add-series/add-series.component";
import { UpdateSeriesComponent } from "./pages/update-series/update-series.component";

export const routes : Routes =[
  {path: '', component:ListSeriesComponent},
  {path: 'create',component:AddSeriesComponent},
  {path: 'edit/:id', component:UpdateSeriesComponent}
];
