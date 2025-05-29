using System.Text.Json.Nodes;

namespace SecureStorage.CQRS.Queries;

public class GetPatientsQuery : MediatR.IRequest<List<JsonObject>>
{
    public string UserId { get; set; }

    public GetPatientsQuery(string userId)
    {
        UserId = userId;
    }
}