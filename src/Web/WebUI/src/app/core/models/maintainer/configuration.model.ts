export interface ConfigurationDto{
    configurationId: string;
    configurationName: string;
    configurationValue: string;
    isActive: boolean;
}

export interface ConfigurationVM{
    configurationId: string;
    configurationName: string;
    configurationValue: string;
}

export interface CreateConfiguration{
    configurationName: string;
    configurationValue: string;
}

export interface UpdateConfiguration{
    configurationId: string;
    configurationName: string;
    configurationValue: string;
}