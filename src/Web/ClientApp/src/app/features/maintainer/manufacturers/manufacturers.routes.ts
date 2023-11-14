import { Routes } from "@angular/router";
import { AddManufacturerComponent } from "./pages/add-manufacturer/add-manufacturer.component";
import { UpdateManufacturerComponent } from "./pages/update-manufacturer/update-manufacturer.component";
import { ManufacturersComponent } from "./manufacturers.component";

export const routes : Routes =[
    {path: '', component:ManufacturersComponent},
    {path: 'create',component:AddManufacturerComponent},
    {path: 'edit/:id', component:UpdateManufacturerComponent}
  ];
  