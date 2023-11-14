import { Routes } from "@angular/router";
import { AddInventoryComponent } from "./pages/add-inventory/add-inventory.component";
import { UpdateInventoryComponent } from "./pages/update-inventory/update-inventory.component";
import { InventoriesComponent } from "./inventories.component";

export const routes : Routes =[
    {path: '', component:InventoriesComponent},
    {path: 'create',component:AddInventoryComponent},
    {path: 'edit/:id', component:UpdateInventoryComponent}
  ];
  