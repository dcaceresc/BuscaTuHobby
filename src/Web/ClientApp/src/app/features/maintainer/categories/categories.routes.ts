import { Routes } from "@angular/router";
import { AddCategoryComponent } from "./pages/add-category/add-category.component";
import { UpdateCategoryComponent } from "./pages/update-category/update-category.component";
import { CategoriesComponent } from "./categories.component";

export const routes : Routes =[
  {path: '', component:CategoriesComponent},
  {path: 'create',component:AddCategoryComponent},
  {path: 'edit/:id', component:UpdateCategoryComponent},
];
