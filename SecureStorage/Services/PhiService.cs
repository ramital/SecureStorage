using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using SecureStorage.Helpers;
using SecureStorage.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureStorage.Services;

public class PhiService : IPhiService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly SecretClient _secretClient;
    private readonly string _containerName = "encrypteddb";
    public readonly OpenFgaClient _fgaClient;

    public PhiService(BlobServiceClient blobServiceClient, SecretClient secretClient,OpenFgaClient fgaClient)
    {
        _blobServiceClient = blobServiceClient;
        _secretClient = secretClient;
        _fgaClient = fgaClient;
    }

    public async Task<Result> StorePhiDataAsync(PhiData phiData)
    {
        try
        {
            string keyName = $"{phiData.PatientKey}-{PatientDataCategoryExtensions.GetGuid((PatientDataCategory)phiData.Category)}";
            if (await DoesKeyExistAsync($"key-{keyName}"))
                return Result.Failure($"{keyName} already exists in Key Vault.");

            // Validate JSON
            JsonSerializer.Deserialize<object>(phiData.Data);

            // Generate AES-256 key
            byte[] key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            // Encrypt data
            string encryptedPhi = SecretStorage.EncryptData(phiData.Data, key);

            // Store in Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient($"{Base64Encode(keyName)}.json");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(encryptedPhi)))
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Store key in Key Vault
            await _secretClient.SetSecretAsync($"key-{keyName}", Convert.ToBase64String(key));

            await  AssignOwnerRolesAsync(keyName);

            return Result.Success($"PHI stored successfully for {keyName}");
        }
        catch (JsonException)
        {
            return Result.Failure("Invalid JSON data.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<object>> RetrievePhiDataAsync(string patientKey)
    {
        try
        {
            // Retrieve key from Key Vault
            var keySecret = await _secretClient.GetSecretAsync($"key-{patientKey}");
            byte[] key = Convert.FromBase64String(keySecret.Value.Value);

            // Retrieve encrypted data from Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient($"{Base64Encode(patientKey)}.json");
            if (!await blobClient.ExistsAsync())
                return Result<object>.Failure("Blob not found.");

            using var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            string encryptedPhi = Encoding.UTF8.GetString(stream.ToArray());

            // Decrypt data
            string decryptedPhi = SecretStorage.DecryptData(encryptedPhi, key);
            var dataObject = JsonSerializer.Deserialize<object>(decryptedPhi);

            return Result<object>.Success(dataObject);
        }
        catch (CryptographicException ex)
        {
            return Result<object>.Failure($"Decryption error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdatePhiDataAsync(PhiData phiData)
    {
        try
        {
            string keyName = $"{phiData.PatientKey}-{PatientDataCategoryExtensions.GetGuid((PatientDataCategory)phiData.Category)}";
            var keySecret = await _secretClient.GetSecretAsync($"key-{keyName}");
            byte[] key = Convert.FromBase64String(keySecret.Value.Value);

            // Encrypt updated data
            string encryptedPhi = SecretStorage.EncryptData(phiData.Data, key);

            // Update Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient($"{Base64Encode(keyName)}.json");
            if (!await blobClient.ExistsAsync())
                return Result.Failure("Blob not found.");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(encryptedPhi)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return Result.Success($"PHI updated successfully for {keyName}");
        }
        catch (CryptographicException ex)
        {
            return Result.Failure($"Encryption error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeletePhiDataAsync(string patientKey)
    {
        try
        {
            // Delete from Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient($"{Base64Encode(patientKey)}.json");
            var deleteResponse = await blobClient.DeleteIfExistsAsync();
            if (!deleteResponse.Value)
                return Result.Failure("Blob not found.");

            // Delete key from Key Vault
            await _secretClient.StartDeleteSecretAsync($"key-{patientKey}");

            return Result.Success("PHI deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error: {ex.Message}");
        }
    }

    private async Task AssignOwnerRolesAsync(string patientId)
    {
        var groups = new[] { "group:nurse", "group:doctor", "group:administrative" };
        var objectId = $"patient:{patientId}";

        var Writes = groups.Select(group => new ClientTupleKey
        {
            User = group,
            Relation = "owner",
            Object = objectId
        }).ToList();

        await _fgaClient.WriteTuples(Writes);
    }

    private async Task<bool> DoesKeyExistAsync(string keyName)
    {
        try
        {
            await _secretClient.GetSecretAsync(keyName);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
}
