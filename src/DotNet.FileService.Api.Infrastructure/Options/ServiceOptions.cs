namespace DotNet.FileService.Api.Infrastructure.Options;

public class ServiceOptions
{
    public const string SectionName = nameof(ServiceOptions);

    public string StorageAccountConnectionString { get; set; } = string.Empty;

    public string BlobContainerName { get; set; } = string.Empty;

    public int SasTokenExpirationMinutes { get; set; }
}
