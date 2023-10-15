import { Routes } from "@angular/router";
import { ListStoresComponent } from "./pages/list-stores/list-stores.component";
import { AddStoreComponent } from "./pages/add-store/add-store.component";
import { UpdateStoreComponent } from "./pages/update-store/update-store.component";
export const routes : Routes =[
  {path: '', component:ListStoresComponent},
  {path: 'create',component:AddStoreComponent},
  {path: 'edit/:id', component:UpdateStoreComponent}
];
