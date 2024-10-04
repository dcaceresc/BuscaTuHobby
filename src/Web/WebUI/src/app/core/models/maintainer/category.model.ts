export interface CategoryDto{
    categoryId:string;
    categoryName:string;
    isActive:boolean;
}

export interface CategoryVM{
    categoryId:string;
    categoryName:string;
}

export interface CreateCategory{
    categoryName:string;
}

export interface UpdateCategory{
    categoryId:string;
    categoryName:string;
}