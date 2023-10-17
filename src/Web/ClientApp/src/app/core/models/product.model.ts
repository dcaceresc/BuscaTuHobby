export interface productDto{
    id:number
    name:string
    active:boolean
}

export interface productVM{
    id:number
    name:string
}

export interface createProductCommand{
    name:string
}