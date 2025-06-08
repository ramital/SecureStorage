using Azure.Data.Tables;
using Azure.Identity;
using Azure.Security.ConfidentialLedger;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenFga.Sdk.Client;
using SecureStorage.CQRS.Queries;
using SecureStorage.Helpers;
using SecureStorage.Repositories;
using SecureStorage.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// CORS for React dev
builder.Services.AddCors(o => o.AddPolicy("AllowReactDev", p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()));

// Keycloak configuration
//builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));
//var keycloakOpts = builder.Configuration.GetSection("Keycloak").Get<KeycloakOptions>()!;



// Azure services
var azureOpts = builder.Configuration.GetSection("Azure").Get<AzureOptions>()!;
builder.Services.AddSingleton(_ => new SecretClient(new Uri(azureOpts.KeyVaultUrl), new DefaultAzureCredential()));
//builder.Services.AddSingleton(_ => new SecretClient(new Uri(azureOpts.ConnectionsVaultUrl), new DefaultAzureCredential()));

var secretClient = new SecretClient(new Uri(azureOpts.ConnectionsVaultUrl), new DefaultAzureCredential());
var storageConnectionString = secretClient.GetSecret("StorageConnectionString").Value.Value;
builder.Services.AddSingleton(_ => new BlobServiceClient(storageConnectionString));

// ConfidentialLedgerClient
builder.Services.AddSingleton(_ => new ConfidentialLedgerClient(new Uri(azureOpts.LedgerEndpoint), new DefaultAzureCredential()));

// TableClient for ConsentIndex
builder.Services.AddSingleton(_ =>
{
    var tableServiceClient = new TableServiceClient(storageConnectionString);
    return tableServiceClient.GetTableClient("ConsentIndex");
});



// Repositories and Services
builder.Services.AddScoped<IBlobRepository, BlobRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IConsentService, ConsentService>();

var keycloakClientId = secretClient.GetSecret("KeycloakClientId").Value.Value;
var keycloakClientSecret = secretClient.GetSecret("KeycloakClientSecret").Value.Value;
var keycloakAuthority = builder.Configuration["Keycloak:Authority"];


var keycloakOpts = new KeycloakOptions
{
    Authority = keycloakAuthority,
    ClientId = keycloakClientId,
    ClientSecret = keycloakClientSecret
};

// Register as singleton for DI
builder.Services.AddSingleton<IOptions<KeycloakOptions>>(Options.Create(keycloakOpts));

// Typed HttpClient for ITokenService
builder.Services
    .AddHttpClient<ITokenService, KeycloakTokenService>()
    .ConfigureHttpClient((sp, client) =>
    { 
        client.BaseAddress = new Uri(keycloakAuthority);
    });


// Authentication and Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakClientId;
        options.RequireHttpsMetadata = false;
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.Error.WriteLine(ctx.Exception);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var StoreId = secretClient.GetSecret("OpenFGAStoreId").Value.Value;
var AuthorizationModelId = secretClient.GetSecret("OpenFGAAuthorizationModelId").Value.Value;

// MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPatientsQuery).Assembly));

// OpenFGA Client
builder.Services.AddSingleton(_ => new OpenFgaClient(new ClientConfiguration
{
    ApiUrl = "http://openfga:8080",
    StoreId = StoreId,
    AuthorizationModelId = AuthorizationModelId
}));

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Secure Storage API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Additional services
builder.Services.AddScoped<IPhiService, PhiService>();

var app = builder.Build();

// Create ConsentIndex table on startup
app.Lifetime.ApplicationStarted.Register(async () =>
{
    var tableClient = app.Services.GetRequiredService<TableClient>();
    await tableClient.CreateIfNotExistsAsync();
});

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();