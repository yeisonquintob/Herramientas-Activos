namespace Navi.ToolsAssets.Application.Documents;

public interface IDocumentStorageService
{
    Task<string> UploadAsync(
        string objectName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string objectName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string objectName,
        CancellationToken cancellationToken = default);
}
