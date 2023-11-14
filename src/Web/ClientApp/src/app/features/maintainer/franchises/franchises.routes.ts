import { Routes } from "@angular/router";
import { AddFranchiseComponent } from "./pages/add-franchise/add-franchise.component";
import { UpdateFranchiseComponent } from "./pages/update-franchise/update-franchise.component";
import { FranchisesComponent } from "./franchises.component";

export const routes : Routes =[
  {path: '', component:FranchisesComponent},
  {path: 'create',component:AddFranchiseComponent},
  {path: 'edit/:id', component:UpdateFranchiseComponent}
];
