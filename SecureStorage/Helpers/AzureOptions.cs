namespace SecureStorage.Helpers
{
    record AzureOptions
    {
        public string BlobStorageConnectionString { get; set; }
        public string KeyVaultUrl { get; set; }
    }
}
