namespace SecureStorage.Helpers;

/// <summary>
/// Represents configuration options for Azure services, including Blob Storage and Key Vault.
/// </summary>
record AzureOptions
{
    /// <summary>
    /// Gets or sets the connection string for Azure Blob Storage.
    /// </summary>
    public string BlobStorageConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the URL for Azure Key Vault.
    /// </summary>
    public string KeyVaultUrl { get; set; }
}
