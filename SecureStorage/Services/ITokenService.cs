namespace SecureStorage.Services
{
    public interface ITokenService
    {
        string GenerateToken(Guid userId,string userName);
        (Guid Id, string Username)? isValidUser(string username, string password);
    }
}
