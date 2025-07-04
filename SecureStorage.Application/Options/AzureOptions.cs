﻿namespace SecureStorage.Application.Options;

/// <summary>
/// Represents configuration options for Azure services, including Blob Storage and Key Vault.
/// </summary>
public record AzureOptions
{
    /// <summary>
    /// Gets or sets the connection string for Ledger Endpoint.
    /// </summary>
    public string LedgerEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the URL for Azure Key Vault.
    /// </summary>
    public string KeyVaultUrl { get; set; }

    /// <summary>
    /// Gets or sets the URL for Azure Key Vault.
    /// </summary>
    public string ConnectionsVaultUrl { get; set; }
}
