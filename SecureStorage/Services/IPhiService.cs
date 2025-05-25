using SecureStorage.Models;

namespace SecureStorage.Services;

public interface IPhiService
{
    Task<Result> StorePhiDataAsync(PhiData phiData);
    Task<Result<object>> RetrievePhiDataAsync(string patientKey);
    Task<Result> UpdatePhiDataAsync(PhiData phiData);
    Task<Result> DeletePhiDataAsync(string patientKey);
}
