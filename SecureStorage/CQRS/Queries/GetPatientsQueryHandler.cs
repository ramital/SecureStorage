using MediatR;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using SecureStorage.Models;
using SecureStorage.Services;
using System.Linq;

namespace SecureStorage.CQRS.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<string>>
{
    public readonly OpenFgaClient _fgaClient;
    private readonly IPhiService _phiService;

    public GetPatientsQueryHandler(OpenFgaClient fgaClient, IPhiService phiService)
    {
        _fgaClient = fgaClient;
        _phiService = phiService;
    }

    public async Task<List<string>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
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
            var results = new List<string>();
            var list = response.Objects.ConvertAll(obj => obj.Replace("patient:", ""));

            foreach (var item in list)
            {
                var phiData = await _phiService.RetrievePhiDataAsync(item);
                results.Add($"{item}-{phiData.Data}");

            }
            return results;

        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving patients: {ex.Message}", ex);
        }
    }
}