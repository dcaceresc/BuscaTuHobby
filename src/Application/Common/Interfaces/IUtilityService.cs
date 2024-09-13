﻿namespace Application.Common.Interfaces;
public interface IUtilityService
{
    string GenerateRandomString(int length);
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    bool IsValidEmail(string email);
}