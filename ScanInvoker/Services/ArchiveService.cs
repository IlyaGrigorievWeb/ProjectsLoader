using ScanInvoker.Interfaces;
using SharpCompress.Archives;

namespace ScanInvoker.Services;

public class ArchiveService : IArchiveService
{
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(ILogger<ArchiveService> logger)
    {
        _logger = logger;
    }

    public async Task<ArchiveExtractionScope> ExtractAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("Archive not found", archivePath);

        var baseDir = Path.GetDirectoryName(archivePath);
        if (baseDir == null)
            throw new Exception("Error when getting the base path");

        try
        {
            await Task.Run(() =>
            {
                using var archive = ArchiveFactory.OpenArchive(archivePath);

                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    entry.WriteToDirectory(baseDir);
                }
            }, cancellationToken).ConfigureAwait(false);
            
            var directories = Directory.GetDirectories(baseDir);
            if (directories.Length == 0)
                throw new Exception($"No directories found after extracting archive {archivePath}");

            var rootDirectory = directories
                .Select(d => new DirectoryInfo(d))
                .OrderByDescending(di => di.CreationTimeUtc)
                .First()
                .FullName;
            
            return new ArchiveExtractionScope(rootDirectory, archivePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Archive parsing error. Project = {archivePath}. {ex.Message}", ex);
        }
    }
}
