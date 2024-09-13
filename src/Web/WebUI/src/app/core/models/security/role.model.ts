export interface RoleDto {
    roleId: string;
    roleName: string;
    isActive: boolean;
}

export interface RoleVM{
    roleId:string;
    roleName:string;
}

export interface CreateRole{
    roleName:string;
}

export interface UpdateRole{
    roleId:string;
    roleName:string;
}