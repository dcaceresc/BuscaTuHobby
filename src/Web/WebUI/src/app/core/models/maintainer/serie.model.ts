export interface SerieDto {
    serieId: string;
    serieName: string;
    franchiseName: string;
    isActive: boolean;
}

export interface SerieVM{
    serieId: string;
    serieName: string;
    franchiseId: string;
}

export interface SerieByFranchiseDto{
    serieId: string | null;
    serieName: string;
}

export interface CreateSerie{
    serieName: string;
    franchiseId: string;
}

export interface UpdateSerie{
    serieId: string;
    serieName: string;
    franchiseId: string;
}
