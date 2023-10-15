import { Routes } from "@angular/router";
import { ListUniversesComponent } from "./pages/list-universes/list-universes.component";
import { AddUniverseComponent } from "./pages/add-universe/add-universe.component";
import { UpdateUniverseComponent } from "./pages/update-universe/update-universe.component";

export const routes : Routes =[
  {path: '', component:ListUniversesComponent},
  {path: 'create',component:AddUniverseComponent},
  {path: 'edit/:id', component:UpdateUniverseComponent}
];
