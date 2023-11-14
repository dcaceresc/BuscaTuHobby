import { Routes } from "@angular/router";
import { AddSerieComponent } from "./pages/add-serie/add-serie.component";
import { UpdateSerieComponent } from "./pages/update-serie/update-serie.component";
import { SeriesComponent } from "./series.component";

export const routes : Routes =[
  {path: '', component:SeriesComponent},
  {path: 'create',component:AddSerieComponent},
  {path: 'edit/:id', component:UpdateSerieComponent}
];
