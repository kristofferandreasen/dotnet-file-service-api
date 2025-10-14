using Azure.Storage.Blobs;
using DotNet.FileService.Api.Authorization;
using DotNet.FileService.Api.Endpoints.V1.Files;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddConfiguredSwagger(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyConstants.BlobReadAccess, policy => policy.RequireRole(RoleConstants.BlobReader))
    .AddPolicy(PolicyConstants.BlobWriteAccess, policy => policy.RequireRole(RoleConstants.BlobWriter))
    .AddPolicy(PolicyConstants.SasTokenReadAccess, policy => policy.RequireRole(RoleConstants.SasTokenReader))
    .AddPolicy(PolicyConstants.SasTokenWriteAccess, policy => policy.RequireRole(RoleConstants.SasTokenWriter));

var blobConnectionString = "blobConnectionString";
var containerName = "FileServiceContainerName";

builder.Services.AddSingleton<IBlobStorageService>(
    _ => new BlobStorageService(new BlobServiceClient(blobConnectionString), containerName));

builder.Services.AddSingleton<ISasTokenService>(
    _ => new SasTokenService(new BlobServiceClient(blobConnectionString), containerName));

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
        c.OAuthClientId(builder.Configuration["AzureAd:SwaggerClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
        c.OAuthAppName("File Service API - Swagger");
    });
}

// Redirect root path to Swagger page
app.MapGet("/", context => Task.Run(()
    => context.Response.Redirect("/swagger/index.html")));

// Map endpoint groups
app.MapUploadEndpoint();
app.MapFileListEndpoint();
app.MapDownloadEndpoint();

app.Run();
