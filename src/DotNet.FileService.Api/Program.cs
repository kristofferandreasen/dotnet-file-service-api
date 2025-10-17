using Azure.Storage.Blobs;
using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Endpoints.V1.Files;
using DotNet.FileService.Api.Endpoints.V1.SasTokens;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Infrastructure.Options;
using DotNet.FileService.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Bind options (for use with IOptions<>)
builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection(AzureAdOptions.SectionName));

// Bind options (for use with IOptions<>)
builder.Services.Configure<ServiceOptions>(
    builder.Configuration.GetSection(ServiceOptions.SectionName));

// Retrieve strongly typed config instances for immediate use
var azureAdOptions = builder.Configuration
    .GetSection(AzureAdOptions.SectionName)
    .Get<AzureAdOptions>()!;

// Retrieve strongly typed config instances for immediate use
var serviceOptions = builder.Configuration
    .GetSection(ServiceOptions.SectionName)
    .Get<ServiceOptions>()!;

builder.Services.ConfigureMicrosoftSecurity(azureAdOptions);

builder.Services.AddAuthorization();
builder.Services.AddConfiguredSwagger(azureAdOptions);
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyConstants.BlobReadAccess, policy => policy.RequireRole(RoleConstants.BlobReader))
    .AddPolicy(PolicyConstants.BlobWriteAccess, policy => policy.RequireRole(RoleConstants.BlobWriter))
    .AddPolicy(PolicyConstants.SasTokenReadAccess, policy => policy.RequireRole(RoleConstants.SasTokenReader))
    .AddPolicy(PolicyConstants.SasTokenWriteAccess, policy => policy.RequireRole(RoleConstants.SasTokenWriter));

builder.Services.AddSingleton<IBlobStorageService>(
    _ => new BlobStorageService(
        new BlobServiceClient(serviceOptions.StorageAccountConnectionString),
        serviceOptions.BlobContainerName));

builder.Services.AddSingleton<ISasTokenService>(
    _ => new SasTokenService(
        new BlobServiceClient(serviceOptions.StorageAccountConnectionString),
        serviceOptions.BlobContainerName));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(azureAdOptions.SwaggerClientId);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
        c.OAuthAppName("File Service API - Swagger");
    });
}

// Redirect root path to Swagger page
app.MapGet("/", context => Task.Run(()
    => context.Response.Redirect("/swagger/index.html")))
    .AllowAnonymous();

// Public health check (no auth)
app.MapGet("/health", () => Results.Ok("OK"))
   .AllowAnonymous();

// Map file endpoints
app.MapUploadEndpoint();
app.MapFileListEndpoint();
app.MapDownloadEndpoint();

// Map sas endpoints
app.MapSasReadEndpoint();
app.MapSasWriteEndpoint();

app.Run();
