export interface productDto{
    id:number
    name:string
    scaleName:string
    manufacturerName:string
    franchiseName:string
    serieName:string | null
    categories :string[]
    active:boolean
}

export interface productVM{
    id:number
    name:string
    scaleId:number
    manufacturerId:number
    franchiseId:number
    serieId:number | null
    hasBase: boolean
    targetAge:string
    size:string
    description:string
    releaseDate:Date
    categories :string[]
}

export interface createProductCommand{
    name:string
    scaleId:number
    manufacturerId:number
    franchiseId:number
    serieId:number | null
    hasBase: boolean
    targetAge:string
    size:string
    description:string
    releaseDate:Date
    categories :number[]
}