export interface ManufacturerDto{
    manufacturerId: string;
    manufacturerName: string;
    isActive: boolean;
}


export interface ManufacturerVM{
    manufacturerId: string;
    manufacturerName: string;
}

export interface CreateManufacturer{
    manufacturerName: string;
}

export interface UpdateManufacturer{
    manufacturerId: string;
    manufacturerName: string;
}