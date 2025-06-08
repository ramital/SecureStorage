using SecureStorage.Application.DTOs;

namespace SecureStorage.Application.Interfaces;

public interface IPhiService
{
    Task<Result> StorePhiDataAsync(PhiData phiData);
    Task<Result<object>> RetrievePhiDataAsync(string patientKey);
    Task<Result> UpdatePhiDataAsync(PhiData phiData);
    Task<Result> DeletePhiDataAsync(string patientKey);
}
