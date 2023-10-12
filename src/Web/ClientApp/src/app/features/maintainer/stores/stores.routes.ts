import { Routes } from "@angular/router";
import { ListStoresComponent } from "./pages/list-stores/list-stores.component";
import { AddStoresComponent } from "./pages/add-stores/add-stores.component";
import { UpdateStoresComponent } from "./pages/update-stores/update-stores.component";
export const routes : Routes =[
  {path: '', component:ListStoresComponent},
  {path: 'create',component:AddStoresComponent},
  {path: 'edit/:id', component:UpdateStoresComponent}
];
