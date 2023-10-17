export interface manufacturerDto{
    id:number
    name:string
    active:boolean
}

export interface manufacturerVM{
    id:number
    name:string
}

export interface createManufacturerCommand{
    name:string
}