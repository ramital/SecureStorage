using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using SecureStorage.Helpers;

namespace SecureStorage.Services
{
    public class KeycloakTokenService : ITokenService
    {
        private readonly HttpClient _http;
        private readonly KeycloakOptions _opts;

        public KeycloakTokenService(HttpClient http, IOptions<KeycloakOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public async Task<string?> GenerateTokenAsync(string username, string password)
        {
            var disco = await _http.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _opts.Authority ,
                Policy =
                {
                    RequireHttps = false 
                }
            });

            var token = await _http.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _opts.ClientId,
                ClientSecret = _opts.ClientSecret,
                UserName = username,
                Password = password,
                Scope = "openid profile"
            });
            if (token.IsError) return null;

            return token?.AccessToken;
        }
     
    }
}
