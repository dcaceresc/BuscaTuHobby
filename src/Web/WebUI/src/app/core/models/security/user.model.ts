export interface UserDto{
    userId:string;
    email:string;
    emailConfirmed:boolean;
    lockoutEnabled:boolean;
    lockoutEnd:Date | null;
    roleNames:string[];
    isActive:boolean;
}