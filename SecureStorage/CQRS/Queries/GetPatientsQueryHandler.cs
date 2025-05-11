using MediatR;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;

namespace SecureStorage.CQRS.Queries
{
    public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<string>>
    {
        public readonly OpenFgaClient _fgaClient;
        public GetPatientsQueryHandler(OpenFgaClient fgaClient)
        {
            _fgaClient = fgaClient;
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

                return response.Objects.ConvertAll(obj => obj.Replace("patient:", ""));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving patients: {ex.Message}", ex);
            }
        }
    }
}