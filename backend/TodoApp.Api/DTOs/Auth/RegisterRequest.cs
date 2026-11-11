namespace TodoApp.Api.DTOs.Auth;

public sealed record RegisterRequest(string Name, string Email, string Password);
