import { Routes } from "@angular/router";
import { UpdateScaleComponent } from "./pages/update-scale/update-scale.component";
import { AddScaleComponent } from "./pages/add-scale/add-scale.component";
import { ListScalesComponent } from "./pages/list-scales/list-scales.component";

export const routes : Routes =[
  {path: '', component:ListScalesComponent},
  {path: 'create',component:AddScaleComponent},
  {path: 'edit/:id', component:UpdateScaleComponent}
];
