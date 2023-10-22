export interface categoryDto{
    id:number;
    name:string;
    groupName:string;
    active:boolean;
}

export interface categoryVM{
    id:number;
    name:string;
    groupId:number;
}



export interface createCategoryCommand{
    name:string;
    groupId:number;
}
