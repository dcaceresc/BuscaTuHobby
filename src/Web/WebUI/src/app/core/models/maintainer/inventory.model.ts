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
    originalPrice: number;
    discountPercentage: number;
    // La condición de la oferta viaja como entero: 0 nueva, 1 reutilizada, 2 usada.
    condition: number;
    paymentMethodLabel: string | null;
}

export interface CreateInventory{
    storeId: string;
    productId: string;
    price: number;
    originalPrice: number;
    discountPercentage: number;
    condition: number;
    paymentMethodLabel: string | null;
}

export interface UpdateInventory{
    inventoryId: string;
    storeId: string;
    productId: string;
    price: number;
    originalPrice: number;
    discountPercentage: number;
    condition: number;
    paymentMethodLabel: string | null;
}
