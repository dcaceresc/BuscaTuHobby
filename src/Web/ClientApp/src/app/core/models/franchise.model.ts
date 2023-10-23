export interface franchiseDto{
    id:number;
    name:string;
    active:boolean;
}

export interface franchiseVM{
    id:number;
    name:string;
}

export interface createFranchiseCommand{
    name:string;
}

