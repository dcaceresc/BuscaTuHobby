import { Routes } from "@angular/router";
import { ListCategoriesComponent } from "./pages/list-categories/list-categories.component";
import { AddCategoryComponent } from "./pages/add-category/add-category.component";
import { UpdateCategoryComponent } from "./pages/update-category/update-category.component";
import { ListSubCategoriesComponent } from "./pages/list-sub-categories/list-sub-categories.component";
import { AddSubCategoryComponent } from "./pages/add-sub-category/add-sub-category.component";
import { UpdateSubCategoryComponent } from "./pages/update-sub-category/update-sub-category.component";

export const routes : Routes =[
  {path: '', component:ListCategoriesComponent},
  {path: 'create',component:AddCategoryComponent},
  {path: 'edit/:id', component:UpdateCategoryComponent},
  {path : ':id/subcategories', component:ListSubCategoriesComponent},
  {path: ':id/subcategories/create', component:AddSubCategoryComponent},
  {path: ':id/subcategories/edit/:subcategoryid', component:UpdateSubCategoryComponent}
];
