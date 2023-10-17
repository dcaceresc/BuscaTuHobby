import { Routes } from "@angular/router";
import { ListManufacturersComponent } from "./pages/list-manufacturers/list-manufacturers.component";
import { AddManufacturerComponent } from "./pages/add-manufacturer/add-manufacturer.component";
import { UpdateManufacturerComponent } from "./pages/update-manufacturer/update-manufacturer.component";

export const routes : Routes =[
    {path: '', component:ListManufacturersComponent},
    {path: 'create',component:AddManufacturerComponent},
    {path: 'edit/:id', component:UpdateManufacturerComponent}
  ];
  