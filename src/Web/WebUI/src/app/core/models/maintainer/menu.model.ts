export interface MenuDto{
    menuId: string;
    menuName: string;
    isActive: boolean;
}

export interface MenuVM{
    menuId: string;
    menuName: string;
}


export interface CreateMenu{
    menuName:string;
}

export interface UpdateMenu{
    menuId:string;
    menuName:string;
}