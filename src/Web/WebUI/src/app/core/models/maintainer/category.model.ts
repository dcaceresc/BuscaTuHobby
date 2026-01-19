export interface CategoryDto{
    categoryId:string;
    categoryName:string;
    categoryIcon:string;
    categoryOrder:number;
    categorySlug:string;
    isActive:boolean;
}

export interface CategoryVM{
    categoryId:string;
    categoryName:string;
    categoryIcon:string;
    categoryOrder:number;
    categorySlug:string;
}

export interface CreateCategory{
    categoryName:string;
    categoryIcon:string;
    categoryOrder:number;
    categorySlug:string;
}

export interface UpdateCategory{
    categoryId:string;
    categoryName:string;
    categoryIcon:string;
    categoryOrder:number;
    categorySlug:string;
}