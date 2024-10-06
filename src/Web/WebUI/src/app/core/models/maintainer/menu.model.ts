export interface MenuDto{
    menuId: string;
    menuName: string;
    isActive: boolean;
}

export interface MenuVM{
    menuId: string;
    menuName: string;
    menuorder: number;
}


export interface CreateMenu{
    menuName:string;
    menuOrder:number;
}

export interface UpdateMenu{
    menuId:string;
    menuName:string;
    menuOrder:number;
}