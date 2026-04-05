namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;

/// <summary>
/// Implementation of file system abstractions.
/// </summary>
public sealed class FileSystemService : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path)
    {
        if (!DirectoryExists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public async Task MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (!FileExists(sourcePath))
            throw new FileNotFoundException($"Source file not found: {sourcePath}");

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (destinationDirectory != null)
        {
            CreateDirectory(destinationDirectory);
        }

        if (FileExists(destinationPath) && !overwrite)
            throw new InvalidOperationException($"Destination file already exists: {destinationPath}");

        if (FileExists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        await Task.Run(() => File.Move(sourcePath, destinationPath, true));
    }

    public async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (!FileExists(sourcePath))
            throw new FileNotFoundException($"Source file not found: {sourcePath}");

        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (destinationDirectory != null)
        {
            CreateDirectory(destinationDirectory);
        }

        await Task.Run(() => File.Copy(sourcePath, destinationPath, overwrite));
    }

    public IReadOnlyCollection<string> GetFiles(string path, string searchPattern = "*.*")
    {
        if (!DirectoryExists(path))
            return new List<string>();

        try
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<byte[]> ReadFileAsync(string path)
    {
        if (!FileExists(path))
            throw new FileNotFoundException($"File not found: {path}");

        return await File.ReadAllBytesAsync(path);
    }

    public string GetUniqueFilename(string directory, string filename)
    {
        if (!FileExists(Path.Combine(directory, filename)))
            return filename;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename);

        int counter = 1;
        while (true)
        {
            var newFilename = $"{nameWithoutExtension}_{counter}{extension}";
            var fullPath = Path.Combine(directory, newFilename);

            if (!FileExists(fullPath))
                return newFilename;

            counter++;
        }
    }
}
