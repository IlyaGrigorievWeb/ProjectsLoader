namespace ScanInvoker.Interfaces;

public interface IArchiveService
{
    Task<string> ExtractAsync(string archivePath, CancellationToken cancellationToken = default);
}
