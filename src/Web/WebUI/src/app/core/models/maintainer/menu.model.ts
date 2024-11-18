export interface MenuDto{
    menuId: string;
    menuName: string;
    menuSlug: string;
    subMenus: string[];
    isActive: boolean;
}

export interface MenuVM{
    menuId: string;
    menuName: string;
    menuOrder: number;
    subMenus: SubMenuDto[];
}


export interface CreateMenu{
    menuName:string;
    menuOrder:number;
    subMenus: CreateSubMenu[];
}

export interface UpdateMenu{
    menuId:string;
    menuName:string;
    menuOrder:number;
    subMenus: UpdateSubMenu[];
}



export interface SubMenuDto{
    subMenuId: string;
    subMenuName: string;
    subMenuOrder: number;
    isActive: boolean;
}

export interface CreateSubMenu{
    subMenuName: string;
    subMenuOrder: number;
}

export interface UpdateSubMenu{
    subMenuId: string;
    subMenuName: string;
    subMenuOrder: number;
}