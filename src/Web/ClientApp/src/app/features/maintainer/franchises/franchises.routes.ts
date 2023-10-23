import { Routes } from "@angular/router";
import { ListFranchisesComponent } from "./pages/list-franchises/list-franchises.component";
import { AddFranchiseComponent } from "./pages/add-franchise/add-franchise.component";
import { UpdateFranchiseComponent } from "./pages/update-franchise/update-franchise.component";

export const routes : Routes =[
  {path: '', component:ListFranchisesComponent},
  {path: 'create',component:AddFranchiseComponent},
  {path: 'edit/:id', component:UpdateFranchiseComponent}
];
