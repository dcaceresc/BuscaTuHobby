import { Routes } from "@angular/router";
import { ListSeriesComponent } from "./pages/list-series/list-series.component";
import { AddSerieComponent } from "./pages/add-serie/add-serie.component";
import { UpdateSerieComponent } from "./pages/update-serie/update-serie.component";

export const routes : Routes =[
  {path: '', component:ListSeriesComponent},
  {path: 'create',component:AddSerieComponent},
  {path: 'edit/:id', component:UpdateSerieComponent}
];
