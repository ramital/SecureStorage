namespace SecureStorage.Application.Interfaces;
    
public interface ITokenService
{
    Task<string?> GenerateTokenAsync(string username, string password);
}
