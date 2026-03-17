namespace ScanInvoker.Interfaces;

public interface IArchiveService
{
    Task<ArchiveExtractionScope> ExtractAsync(string archivePath, CancellationToken cancellationToken = default);
}
