﻿namespace Domain.Common;
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
}


public class ApiResponse<T> : ApiResponse
{
    public T Data { get; set; } = default!;
}
