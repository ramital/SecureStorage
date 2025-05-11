using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenFga.Sdk.Client;
using SecureStorage.CQRS.Queries;
using SecureStorage.Helpers;
using SecureStorage.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

});
// Add services to the container.
builder.Services.AddAuthorization();
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Secure Storage API", Version = "v1" });

    // Add JWT Bearer Auth
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSingleton<ITokenService, TokenService>();  
builder.Services.AddScoped<IPhiService, PhiService>();
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
