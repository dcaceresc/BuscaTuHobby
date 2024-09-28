export interface StoreDto{
    storeId:string;
    storeName:string;
    storeAddress:string[];
    storeWebSite:string;
    isActive:boolean;
}

export interface StoreVM{
    storeId:string;
    storeName:string;
    storeAddress: StoreAddressDto[];
    storeWebSite:string;
}

export interface StoreAddressDto{
    storeId:string;
    storeAddressId:string;
    regionId:string;
    street:string;
    communeId:string;
    zipCode:string | null;
}

export interface CreateStore{
    storeName:string;
    storeWebSite:string;
}

export interface CreateStoreAddress{
    storeId:string;
    street:string;
    communeId:string;
    zipCode:string | null;
}

export interface UpdateStore{
    storeId:string;
    storeName:string;
    storeAddress:string;
    storeWebSite:string;
}

export interface UpdateStoreAddress{
    storeAddressId:string;
    storeId:string;
    street:string;
    communeId:string;
    zipCode:string | null;
}