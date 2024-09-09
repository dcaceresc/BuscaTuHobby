export interface UserLogin{
    email: string;
    password: string;
}

export interface UserRegister{
    email: string;
    password: string;
    confirmPassword: string;
    acceptTerms: boolean;
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