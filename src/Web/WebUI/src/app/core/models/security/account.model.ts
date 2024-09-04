export interface UserLogin{
    email: string;
    password: string;
}

export interface AdminLoginRequestCommand{
    userName: string;
    password: string;
    supplanted: string;
}

export interface UserTokenResponse{
    accessToken: string;
    refreshToken: string;
}