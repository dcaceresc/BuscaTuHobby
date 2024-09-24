export interface InventoryDto {
    inventoryId: string;
    productName: string;
    storeName: string;
    price: number;
    isActive: boolean;
}

export interface InventoryVM {
    inventoryId: string;
    storeId: string;
    productId: string;
    price: number;
}

export interface CreateInventory{
    storeId: string;
    productId: string;
    price: number;
}

export interface UpdateInventory{
    inventoryId: string;
    storeId: string;
    productId: string;
    price: number;
}