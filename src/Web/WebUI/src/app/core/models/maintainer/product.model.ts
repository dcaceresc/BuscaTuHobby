export interface ProductDto{
    productId:string;
    productName:string;
    scaleName:string;
    manufacturerName:string;
    franchiseName:string;
    serieName:string;
    categories:string[];
    isActive:boolean;
}


export interface ProductVM{
    productId:string;
    productName:string;
    scaleId:string;
    manufacturerId:string;
    franchiseId:string;
    serieId:string;
    productHasBase:boolean;
    productTargetAge:string;
    productSize:string;
    productDescription:string;
    productReleaseDate:string;
    categoryIds:string[];
    productImages:string[];
}


export interface CreateProduct{
    productName:string;
    scaleId:string;
    manufacturerId:string;
    franchiseId:string;
    serieId:string;
    productHasBase:boolean;
    productTargetAge:string;
    productSize:string;
    productDescription:string;
    productReleaseDate:string;
    categoryIds:string[];
    productImages:string[];
}