export interface FranchiseDto {
    franchiseId:string;
    franchiseName:string;
    isActive:boolean;
}

export interface FranchiseVM{
    franchiseId:string;
    franchiseName:string;
}

export interface CreateFranchise{
    franchiseName:string;
}

export interface UpdateFranchise{
    franchiseId:string;
    franchiseName:string;
}