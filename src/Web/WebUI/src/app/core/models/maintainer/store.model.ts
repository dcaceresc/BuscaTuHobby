export interface StoreDto{
    storeId:string;
    storeName:string;
    storeAddress:string;
    storeWebSite:string;
    isActive:boolean;
}

export interface StoreVM{
    storeId:string;
    storeName:string;
    storeAddress:string;
    storeWebSite:string;
}

export interface CreateStore{
    storeName:string;
    storeAddress:string;
    storeWebSite:string;
}

export interface UpdateStore{
    storeId:string;
    storeName:string;
    storeAddress:string;
    storeWebSite:string;
}