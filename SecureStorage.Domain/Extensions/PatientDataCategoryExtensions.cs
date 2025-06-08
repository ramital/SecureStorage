using SecureStorage.Domain.Enums;

namespace SecureStorage.Domain.Models;

public static class PatientDataCategoryExtensions
{
    private static readonly Dictionary<PatientDataCategory, Guid> _categoryGuids = new()
    {
        { PatientDataCategory.Identifiers,    new Guid("b730fc88-4c8c-4af0-9375-1655e0c49126") },
        { PatientDataCategory.MedicalRecords, new Guid("d138af2e-f265-4bce-9324-f4892558f6fc") },
        { PatientDataCategory.FinancialInfo,  new Guid("59d7a4ed-b041-4f10-b403-c112be2e847e") },
        { PatientDataCategory.ContactInfo,    new Guid("85d0e5db-b10e-45e7-a7e1-85c8bfe48e61") },
        { PatientDataCategory.InsuranceInfo,  new Guid("768d2fbd-f216-4b96-a221-5013a18ecf5c") },
        { PatientDataCategory.BiometricData,  new Guid("71df5d4b-19c7-4bb0-8796-2ce0ff444b91") },
    };

    private static readonly Dictionary<Guid, PatientDataCategory> _guidToCategory =
        _categoryGuids.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static Guid GetGuid(this PatientDataCategory category)
        => _categoryGuids[category];

    public static PatientDataCategory GetCategory(this Guid guid)
    => _guidToCategory.TryGetValue(guid, out var cat)
        ? cat  : throw new ArgumentException($"No category found for GUID: {guid}", nameof(guid));
}
