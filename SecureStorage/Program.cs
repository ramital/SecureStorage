using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using OpenFga.Sdk.Client;
using SecureStorage.CQRS.Queries;
using SecureStorage.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind Azure options
var azureOptions = builder.Configuration.GetSection("Azure").Get<AzureOptions>();

builder.Services.AddSingleton(_ => new BlobServiceClient(azureOptions.BlobStorageConnectionString));
builder.Services.AddSingleton(_ => new SecretClient(new Uri(azureOptions.KeyVaultUrl), new DefaultAzureCredential()));


// Register MediatR for CQRS
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetPatientsQuery).Assembly));


// Register OpenFGA client as a singleton
builder.Services.AddSingleton<OpenFgaClient>(_ =>
{
    var configuration = new ClientConfiguration
    {
        ApiUrl = "http://openfga:8080",
        StoreId = "01JTY2QWF3Z2ND8KJ9GXV239RY",
        AuthorizationModelId = "01JTYKMGV5N5QQ3GPM8B02FZSQ"
    };
    return new OpenFgaClient(configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
