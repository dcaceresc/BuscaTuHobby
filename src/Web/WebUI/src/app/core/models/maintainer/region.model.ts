export interface RegionDto{
    regionId:string;
    regionName:string;
    regionOrder:number;
    isActive:boolean;
}


export interface RegionVM{
    regionId:string;
    regionName:string;
    regionOrder:number;
}


export interface CreateRegion{
    regionName:string;
    regionOrder:number;
}

export interface UpdateRegion{
    regionId:string;
    regionName:string;
    regionOrder:number;
}