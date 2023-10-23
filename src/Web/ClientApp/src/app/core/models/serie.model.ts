export interface serieDto{
  id:number
  name:string
  franchiseName:string
  active:boolean
}

export interface serieByFranchiseDto{
  id:number
  name:string
}

export interface serieVM{
  id:number
  name:string
  franchiseId:number
}

export interface createSerieCommnad{
  name:string
  franchiseId:number
}