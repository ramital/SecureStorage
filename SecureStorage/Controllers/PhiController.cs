using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Helpers;
using SecureStorage.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureStorage.Controllers
{
    [ApiController]
    [Route("phi")]
    public class PhiController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly SecretClient _secretClient;
        private readonly string _containerName = "encrypteddb";

        public PhiController(BlobServiceClient blobServiceClient, SecretClient secretClient)
        {
            _blobServiceClient = blobServiceClient;
            _secretClient = secretClient;
        }

        [HttpPost("store")]
        public async Task<IActionResult> StorePhiData([FromBody] PhiData phiData)
        {
            try
            {
                string keyName = $"{phiData.PatientKey}-{(PatientDataCategory)phiData.Category}";

                if (await DoesKeyExistAsync($"key-{keyName}"))
                {
                    return NotFound(new { Message = $"{keyName} already exists in Key Vault." });
                }

                JsonSerializer.Deserialize<object>(phiData.Data);

                // Step 1: Generate a random AES-256 key for the patient
                byte[] key = new byte[32]; // 256 bits
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(key);
                }

                // Step 2: Encrypt the PHI data with the key
                string encryptedPhi = SecretStorage.EncryptData(phiData.Data, key);

                // Step 3: Store the encrypted PHI in Azure Blob Storage
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient($"{keyName}.json");
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(encryptedPhi)))
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // Step 4: Store the key in Azure Key Vault
                await _secretClient.SetSecretAsync($"key-{keyName}", Convert.ToBase64String(key));

                return Ok(new { Message = "PHI stored successfully", BlobName = blobClient.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("retrieve")]
        public async Task<IActionResult> RetrievePhiData(string patientKey)
        {
            try
            {
                // Step 1: Retrieve the key from Azure Key Vault
                var keySecret = await _secretClient.GetSecretAsync($"key-{patientKey}");
                byte[] keyByte = Convert.FromBase64String(keySecret.Value.Value);

                // Step 2: Retrieve the encrypted PHI from Azure Blob Storage
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient($"{patientKey}.json");
                using var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                string encryptedPhi = Encoding.UTF8.GetString(stream.ToArray());

                // Step 3: Decrypt the PHI
                string decryptedPhi = SecretStorage.DecryptData(encryptedPhi, keyByte);

                // Step 4: Return the decrypted data to the client
                var dataObject = JsonSerializer.Deserialize<object>(decryptedPhi);

                // Step 5: Return the response with prettified Data
                return Ok(new { Data = dataObject });
            }
            catch (CryptographicException ex)
            {
                return StatusCode(500, $"Decryption error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePhiData([FromBody] PhiData phiData)
        {
            try
            {
                // Step 1: Retrieve the existing key from Azure Key Vault
                var keySecret = await _secretClient.GetSecretAsync($"key-{phiData.PatientKey}-{(PatientDataCategory)phiData.Category}");
                byte[] key = Convert.FromBase64String(keySecret.Value.Value);

                // Step 2: Encrypt the updated PHI data with the same key
                string encryptedPhi = SecretStorage.EncryptData(phiData.Data, key);

                // Step 3: Overwrite the existing blob in Azure Blob Storage
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient($"{phiData.PatientKey}-{(PatientDataCategory)phiData.Category}.json");
                if (!await blobClient.ExistsAsync())
                {
                    return NotFound(new { Message = "Blob not found." });
                }

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(encryptedPhi)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return Ok(new { Message = "PHI updated successfully", BlobName = blobClient.Name });
            }
            catch (CryptographicException ex)
            {
                return StatusCode(500, $"Encryption error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePhiData(string patientKey)
        {
            try
            {
                // Step 1: Delete the blob from Azure Blob Storage
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient($"{patientKey}.json");
                var deleteResponse = await blobClient.DeleteIfExistsAsync();

                if (!deleteResponse.Value)
                {
                    return NotFound(new { Message = "Blob not found." });
                }

                await _secretClient.StartDeleteSecretAsync($"key-{patientKey}");
                

                return Ok(new { Message = "PHI deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
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
    }
}