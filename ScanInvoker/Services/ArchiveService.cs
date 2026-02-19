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

    public async Task<string> ExtractAsync(string archivePath, CancellationToken cancellationToken = default)
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
            }, cancellationToken);

            File.Delete(archivePath);
            
            var rootDirectory = Directory.GetDirectories(baseDir).Single();

            return rootDirectory;
        }
        catch
        {
            throw new Exception("Archive parsing error");
        }
    }
}
