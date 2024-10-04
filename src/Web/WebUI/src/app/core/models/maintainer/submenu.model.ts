export interface SubMenuDto{
    subMenuId: string;
    subMenuName: string;
    menuName: string;
    isActive: boolean;
}

export interface SubMenuVM{
    subMenuId: string;
    subMenuName: string;
    menuId: string;
}

export interface CreateSubMenu{
    subMenuName: string;
    menuId: string;
}

export interface UpdateSubMenu{
    subMenuId: string;
    subMenuName: string;
    menuId: string;
}