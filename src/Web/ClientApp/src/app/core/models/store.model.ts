export interface storeDto{
    id:number,
    name:string,
    address:string,
    webSite:string,
    active:boolean
}

export interface createStoreCommand{
    name:string,
    address:string,
    webSite:string
}

export interface storeVM{
    id:number,
    name:string,
    address:string,
    webSite:string,
}