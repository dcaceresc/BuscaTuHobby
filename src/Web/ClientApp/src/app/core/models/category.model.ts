export interface categoryDto{
    id:number;
    name:string;
    active:boolean;
}

export interface subCategoryDto{
    id:number;
    name:string;
    categoryId:number;
    active:boolean;
}

export interface categoryVM{
    id:number;
    name:string;
}

export interface subCategoryVM{
    id:number;
    name:string;
    categoryId:number;
}

export interface createCategoryCommand{
    name:string;
}

export interface createSubCategoryCommand{
    name:string;
    categoryId:number;
}