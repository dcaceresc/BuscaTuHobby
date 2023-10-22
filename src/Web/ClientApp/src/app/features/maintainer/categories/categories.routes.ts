import { Routes } from "@angular/router";
import { ListCategoriesComponent } from "./pages/list-categories/list-categories.component";
import { AddCategoryComponent } from "./pages/add-category/add-category.component";
import { UpdateCategoryComponent } from "./pages/update-category/update-category.component";

export const routes : Routes =[
  {path: '', component:ListCategoriesComponent},
  {path: 'create',component:AddCategoryComponent},
  {path: 'edit/:id', component:UpdateCategoryComponent},
];
