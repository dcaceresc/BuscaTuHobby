import { Routes } from "@angular/router";
import { ListUniversesComponent } from "./pages/list-universes/list-universes.component";
import { AddUniversesComponent } from "./pages/add-universes/add-universes.component";
import { UpdateUniversesComponent } from "./pages/update-universes/update-universes.component";

export const routes : Routes =[
  {path: '', component:ListUniversesComponent},
  {path: 'create',component:AddUniversesComponent},
  {path: 'edit/:id', component:UpdateUniversesComponent}
];
