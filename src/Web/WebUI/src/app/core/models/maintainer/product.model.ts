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


/**
 * Grado Gunpla que publica la API como entero. Las etiquetas son las mismas que publican los facets del catálogo,
 * para que el mantenedor y el sitio no usen dos vocabularios distintos.
 */
export const GUNPLA_GRADE_OPTIONS = [
    { value: 0, label: 'Sin grado' },
    { value: 1, label: 'High Grade (HG)' },
    { value: 2, label: 'Master Grade (MG)' },
    { value: 3, label: 'Real Grade (RG)' },
    { value: 4, label: 'Perfect Grade (PG)' },
    { value: 5, label: 'Super Deformed (SD)' },
    { value: 6, label: 'Otro' }
] as const;

/** Etiqueta del grado; 0 significa que el producto no tiene grado asignado y se omite en las fichas. */
export function gunplaGradeLabel(grade: number): string {
    return GUNPLA_GRADE_OPTIONS.find(option => option.value === grade)?.label ?? 'Sin dato';
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
    // Campos estructurados del catálogo Gunpla: la API rechaza una escala o un número de parte vacíos,
    // así que null es la forma de decir "no informado".
    grade:number;
    scale:string | null;
    partNumber:string | null;
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
    grade:number;
    scale:string | null;
    partNumber:string | null;
    productDescription:string;
    productReleaseDate:string;
    categoryIds:string[];
    productImages:string[];
}
