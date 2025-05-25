namespace SecureStorage.Services;

public interface ITokenService
{
    Task<string?> GenerateTokenAsync(string username, string password);
}
