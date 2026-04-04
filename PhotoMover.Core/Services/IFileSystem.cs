namespace PhotoMover.Core.Services;

/// <summary>
/// Abstract file system operations for testability.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Checks if a file exists at the given path.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Checks if a directory exists at the given path.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory and all parent directories if needed.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Moves a file from source to destination.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="destinationPath">Destination file path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    Task MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Copies a file from source to destination.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="destinationPath">Destination file path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Gets all files in a directory matching a pattern.
    /// </summary>
    IReadOnlyCollection<string> GetFiles(string path, string searchPattern = "*.*");

    /// <summary>
    /// Reads file bytes asynchronously.
    /// </summary>
    Task<byte[]> ReadFileAsync(string path);

    /// <summary>
    /// Gets the unique filename by appending a number if needed.
    /// </summary>
    string GetUniqueFilename(string directory, string filename);
}
