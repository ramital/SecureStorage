using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenFga.Sdk.Client;
using SecureStorage.CQRS.Queries;
using SecureStorage.Helpers;
using SecureStorage.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Bind Keycloak settings
builder.Services.Configure<KeycloakOptions>(
  builder.Configuration.GetSection("Keycloak"));
var keycloakOpts = builder.Configuration
  .GetSection("Keycloak")
  .Get<KeycloakOptions>()!;

// 2) Typed HttpClient for ITokenService
builder.Services
  .AddHttpClient<ITokenService, KeycloakTokenService>()
  .ConfigureHttpClient((sp, client) =>
  {
      client.BaseAddress = new Uri(
        sp.GetRequiredService<IOptions<KeycloakOptions>>().Value.Authority
      );
  });

// 3) JWT Bearer for incoming tokens
builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.Authority = keycloakOpts.Authority;
      options.Audience = keycloakOpts.ClientId;
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
//JWT Login for testing
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };

//});
//builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var azureOpts = builder.Configuration.GetSection("Azure").Get<AzureOptions>()!;
builder.Services.AddSingleton(_ =>
  new BlobServiceClient(azureOpts.BlobStorageConnectionString));
builder.Services.AddSingleton(_ =>
  new SecretClient(new Uri(azureOpts.KeyVaultUrl), new DefaultAzureCredential()));

builder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssembly(typeof(GetPatientsQuery).Assembly));

builder.Services.AddSingleton(_ =>
  new OpenFgaClient(new ClientConfiguration
  {
      ApiUrl = "http://openfga:8080",
      StoreId = "01JTY2QWF3Z2ND8KJ9GXV239RY",
      AuthorizationModelId = "01JTYKMGV5N5QQ3GPM8B02FZSQ"
  }));

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
        Description = "Enter ‘Bearer {token}’"
    });
    c.AddSecurityRequirement(new() {
    {
      new() {
        Reference = new() {
          Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
          Id   = "Bearer"
        }
      },
      new string[] { }
    }
  });
});

builder.Services.AddScoped<IPhiService, PhiService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
