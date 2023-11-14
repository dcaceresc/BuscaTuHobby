import { Routes } from "@angular/router";
import { AddStoreComponent } from "./pages/add-store/add-store.component";
import { UpdateStoreComponent } from "./pages/update-store/update-store.component";
import { StoresComponent } from "./stores.component";

export const routes : Routes =[
  {path: '', component:StoresComponent},
  {path: 'create',component:AddStoreComponent},
  {path: 'edit/:id', component:UpdateStoreComponent}
];
