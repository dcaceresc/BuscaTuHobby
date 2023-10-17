import { Routes } from "@angular/router";
import { ListProductsComponent } from "./pages/list-products/list-products.component";
import { AddProductComponent } from "./pages/add-product/add-product.component";
import { UpdateProductComponent } from "./pages/update-product/update-product.component";

export const routes : Routes =[
    {path: '', component:ListProductsComponent},
    {path: 'create',component:AddProductComponent},
    {path: 'edit/:id', component:UpdateProductComponent}
  ];
  