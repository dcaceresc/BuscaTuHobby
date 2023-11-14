import { Routes } from "@angular/router";
import { AddGroupComponent } from "./pages/add-group/add-group.component";
import { UpdateGroupComponent } from "./pages/update-group/update-group.component";
import { GroupsComponent } from "./groups.component";

export const routes : Routes =[
  {path: '', component:GroupsComponent},
  {path: 'create',component:AddGroupComponent},
  {path: 'edit/:id', component:UpdateGroupComponent}
];
