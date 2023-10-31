import { Routes } from "@angular/router";
import { ListInventoriesComponent } from "./pages/list-inventories/list-inventories.component";
import { AddInventoryComponent } from "./pages/add-inventory/add-inventory.component";
import { UpdateInventoryComponent } from "./pages/update-inventory/update-inventory.component";

export const routes : Routes =[
    {path: '', component:ListInventoriesComponent},
    {path: 'create',component:AddInventoryComponent},
    {path: 'edit/:id', component:UpdateInventoryComponent}
  ];
  