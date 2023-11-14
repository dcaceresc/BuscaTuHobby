import { Routes } from "@angular/router";
import { AddProductComponent } from "./pages/add-product/add-product.component";
import { UpdateProductComponent } from "./pages/update-product/update-product.component";
import { ProductsComponent } from "./products.component";

export const routes : Routes =[
    {path: '', component:ProductsComponent},
    {path: 'create',component:AddProductComponent},
    {path: 'edit/:id', component:UpdateProductComponent}
  ];
  