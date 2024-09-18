export interface ScaleDto{
    scaleId: string;
    scaleName: string;
    isActive: boolean;
}

export interface ScaleVM{
    scaleId: string;
    scaleName: string;
}

export interface CreateScale{
    scaleName: string;
}

export interface UpdateScale{
    scaleId: string;
    scaleName: string;
}