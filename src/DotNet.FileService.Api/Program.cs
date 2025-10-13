using DotNet.FileService.Api.Endpoints.v1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoint groups
app.MapUploadEndpoint();
app.MapFileListEndpoint();
app.MapDownloadEndpoint();

app.Run();
