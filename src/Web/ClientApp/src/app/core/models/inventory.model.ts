export interface inventoryDto{
    id:number;
    productName:string;
    storeName:string;
    price:number;
    active:boolean;
}

export interface inventoryVM{
    id:number
    productId:number
    storeId:number
    price:number
}

export interface createInventoryCommand{
    productId:number
    storeId:number
    price:number
}