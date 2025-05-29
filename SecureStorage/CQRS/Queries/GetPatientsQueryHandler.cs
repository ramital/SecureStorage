using MediatR;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using SecureStorage.Models;
using SecureStorage.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SecureStorage.CQRS.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<JsonObject>>
{
    public readonly OpenFgaClient _fgaClient;
    private readonly IPhiService _phiService;

    public GetPatientsQueryHandler(OpenFgaClient fgaClient, IPhiService phiService)
    {
        _fgaClient = fgaClient;
        _phiService = phiService;
    }

    public async Task<List<JsonObject>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new ArgumentException("userId is required.");
        }

        try
        {
            var fgaRequest = new ClientListObjectsRequest
            {
                User = $"user:{request.UserId}",
                Relation = "can_read",
                Type = "patient"
            };

            var response = await _fgaClient.ListObjects(fgaRequest);
            var results = new List<PatientDataEntry>();
            var list = response.Objects.ConvertAll(obj => obj.Replace("patient:", ""));

            foreach (var item in list)
            {
                var phiData = await _phiService.RetrievePhiDataAsync(item);

               var data = parseData(item);
                results.Add(new PatientDataEntry( data.Item1, data.Item2, JsonSerializer.Serialize(phiData.Data)));

            }

            return BuildPatientPayloads(results);

        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving patients: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Builds a list of JSON objects representing patient data entries grouped by user ID.
    /// Each JSON object contains the user ID and the associated patient data categories as properties.
    /// </summary>
    /// <param name="entries">A list of patient data entries to group and serialize.</param>
    /// <returns>A list of JsonObject instances, each representing a user's patient data.</returns>
    private List<JsonObject> BuildPatientPayloads(List<PatientDataEntry> entries)
    {
        return entries
          .GroupBy(e => e.UserId)
          .Select(g =>
          {
              var obj = new JsonObject
              {
                  ["UserId"] = g.Key.ToString()
              };

              foreach (var e in g)
                  obj[e.Category.ToString()] = JsonNode.Parse(e.DataJson);

              return obj;
          })
          .ToList();
    }
    /// <summary>
    /// Parses a data string to extract the user ID and patient data category.
    /// The input string is expected to contain a 36-character GUID for the user ID,
    /// followed by a separator and another GUID representing the category.
    /// </summary>
    /// <param name="data">The data string containing the user ID and category GUID.</param>
    /// <returns>A tuple containing the parsed user ID (Guid) and the corresponding PatientDataCategory.</returns>
    private (Guid, PatientDataCategory) parseData(string data)
    {
        var userIdStr = data[..36];
        var categoryGuidStr = data[(36 + 1)..];
        var userId = Guid.Parse(userIdStr);
        var categoryGuid = PatientDataCategoryExtensions.GetCategory(Guid.Parse(categoryGuidStr));
        return (userId, categoryGuid);
    }
    private record PatientDataEntry(
    Guid UserId,
    PatientDataCategory Category,
    string DataJson
);
}