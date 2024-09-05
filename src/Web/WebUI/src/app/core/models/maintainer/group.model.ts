export interface GroupDto{
    groupId: string;
    groupName: string;
    isActive: boolean;
}


export interface CreaterGroup{
    groupName:string;
}

export interface UpdateGroup{
    groupId:string;
    groupName:string;
}