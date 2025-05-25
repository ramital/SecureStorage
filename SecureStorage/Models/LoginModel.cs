namespace SecureStorage.Models;

/// <summary>
/// Represents the login credentials for a user.
/// </summary>
public class LoginModel
{
    /// <summary>
    /// Gets or sets the username for the user.
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// Gets or sets the password for the user.
    /// </summary>
    public string Password { get; set; }
}
