import { Routes } from "@angular/router";
import { ListGroupsComponent } from "./pages/list-groups/list-groups.component";
import { AddGroupComponent } from "./pages/add-group/add-group.component";
import { UpdateGroupComponent } from "./pages/update-group/update-group.component";

export const routes : Routes =[
  {path: '', component:ListGroupsComponent},
  {path: 'create',component:AddGroupComponent},
  {path: 'edit/:id', component:UpdateGroupComponent}
];
