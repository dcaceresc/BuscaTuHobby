export interface CommuneDto {
    communeId:string;
    communeName:string;
    regionName:string;
    isActive:boolean;
}

export interface CommuneByRegion{
    communeId:string;
    communeName:string;
}

export interface CommuneVM{
    communeId:string;
    communeName:string;
    regionId:string;
}

export interface CreateCommune{
    communeName:string;
    regionId:string;
}

export interface UpdateCommune{
    communeId:string;
    communeName:string;
    regionId:string;
}
