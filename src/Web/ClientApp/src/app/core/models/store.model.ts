export interface storeDto{
    id:number,
    name:string,
    address:string,
    ranking:number,
    active:boolean
}

export interface createStoreCommand{
    name:string,
    address:string,
    ranking:number
}