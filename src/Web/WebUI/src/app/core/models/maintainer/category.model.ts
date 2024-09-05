export interface CategoryDto{
    categoryId:string;
    categoryName:string;
    groupName:string;
    isActive:boolean;
}

export interface CategoryVM{
    categoryId:string;
    categoryName:string;
    groupId:string;
}

export interface CreateCategory{
    categoryName:string;
    groupId:string;
}

export interface UpdateCategory{
    categoryId:string;
    categoryName:string;
    groupId:string;
}