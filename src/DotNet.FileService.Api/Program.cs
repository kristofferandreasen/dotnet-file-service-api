using Azure.Storage.Blobs;
using DotNet.FileService.Api.Endpoints.V1;
using DotNet.FileService.Api.Infrastructure.BlobStorage;
using DotNet.FileService.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddConfiguredSwagger();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ReadAccess", policy => policy.RequireRole("Read"))
    .AddPolicy("WriteAccess", policy => policy.RequireRole("Write"));

var blobConnectionString = "blobConnectionString";
var containerName = "FileServiceContainerName";

builder.Services.AddSingleton<IBlobStorageService>(
    _ => new BlobStorageService(new BlobServiceClient(blobConnectionString), containerName));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect root path to Swagger page
app.MapGet("/", context => Task.Run(()
    => context.Response.Redirect("/swagger/index.html")));

// Map endpoint groups
app.MapUploadEndpoint();
app.MapFileListEndpoint();
app.MapDownloadEndpoint();

app.Run();
