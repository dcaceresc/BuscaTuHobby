export interface UserDto{
    userId:string;
    email:string;
    emailConfirmed:boolean;
    lockoutEnabled:boolean;
    lockoutEnd:Date | null;
    roleNames:string[];
    isActive:boolean;
}

export interface UserVM{
    userId:string;
    email:string;
    emailConfirmed:boolean;
    lockoutEnabled:boolean;
    lockoutEnd:Date | null;
    roleIds:string[];
}


export interface CreateUser{
    email:string;
    roleIds:string[];
}