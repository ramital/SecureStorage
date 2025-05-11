namespace SecureStorage.CQRS.Queries;

public class GetPatientsQuery : MediatR.IRequest<List<string>>
{
    public string UserId { get; set; }

    public GetPatientsQuery(string userId)
    {
        UserId = userId;
    }
}