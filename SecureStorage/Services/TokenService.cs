  using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Security;
using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

namespace SecureStorage.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Guid userId, string userName)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, userName) 
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public (Guid Id, string Username)? isValidUser(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user == default)
                return null;

            return (user.Id, user.Username);
        }

        private static readonly List<(Guid Id, string Username,string Password)> _users = new()
        {
            (new Guid("88faa0d2-aefc-49c9-b651-f59d58e54384"),Username: "alice",Password: "test"),//Administrative 
            (new Guid("b570c3bb-6825-4b88-81d9-8989426fe41e"),Username: "bob",Password: "test"),//Nurse 
            (new Guid("3a53bd1d-4876-4867-9c68-9c9239963107"),Username:"joe",Password: "test"),//Doctor 
        };

    }
}
