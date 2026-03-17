public sealed class ArchiveExtractionScope : IAsyncDisposable
{
    public string FolderPath { get; }
    private readonly string _archivePath;

    public ArchiveExtractionScope(string folderPath, string archivePath)
    {
        FolderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        _archivePath = archivePath;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(FolderPath))
                    Directory.Delete(FolderPath, recursive: true);

                if (!string.IsNullOrEmpty(_archivePath) && File.Exists(_archivePath))
                    File.Delete(_archivePath);

            }).ConfigureAwait(false);
        }
        catch
        {
        }
    }
}