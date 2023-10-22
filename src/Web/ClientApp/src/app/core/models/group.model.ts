export interface groupDto{
    id:number;
    name:string;
    active:boolean;
}

export interface groupVM{
    id:number;
    name:string;
}

export interface createGroupCommand{
    name:string;
}

