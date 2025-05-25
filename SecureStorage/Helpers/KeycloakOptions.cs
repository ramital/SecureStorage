namespace SecureStorage.Helpers;

/// <summary>
/// Represents configuration options for connecting to a Keycloak authentication server.
/// </summary>
public class KeycloakOptions
{
    /// <summary>
    /// Gets or sets the authority URL of the Keycloak server.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Gets or sets the client ID registered with Keycloak.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret associated with the client ID.
    /// </summary>
    public string ClientSecret { get; set; }
}
