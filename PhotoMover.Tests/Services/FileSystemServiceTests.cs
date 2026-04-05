namespace PhotoMover.Tests.Services;

using PhotoMover.Infrastructure.Services;
using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the FileSystemService.
/// </summary>
public sealed class FileSystemServiceTests : IDisposable
{
    private readonly FileSystemService _sut = new();
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), $"PhotoMoverTest_{Guid.NewGuid()}");

    public FileSystemServiceTests()
    {
        if (!Directory.Exists(_testDirectory))
        {
            Directory.CreateDirectory(_testDirectory);
        }
    }

    [Fact]
    public void FileExists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(testFile, "test content");

        // Act
        var result = _sut.FileExists(testFile);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileExists_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = _sut.FileExists(testFile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
    {
        // Act
        var result = _sut.DirectoryExists(_testDirectory);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_WithNonExistentDirectory_ReturnsFalse()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        // Act
        var result = _sut.DirectoryExists(nonExistentDir);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateDirectory_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var newDir = Path.Combine(_testDirectory, "newdir");

        // Act
        _sut.CreateDirectory(newDir);

        // Assert
        Directory.Exists(newDir).Should().BeTrue();
    }

    [Fact]
    public async Task MoveFileAsync_MovesFileToDestination()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "source.txt");
        var destDir = Path.Combine(_testDirectory, "dest");
        var destFile = Path.Combine(destDir, "source.txt");

        File.WriteAllText(sourceFile, "test content");
        Directory.CreateDirectory(destDir);

        // Act
        await _sut.MoveFileAsync(sourceFile, destFile);

        // Assert
        File.Exists(destFile).Should().BeTrue();
        File.Exists(sourceFile).Should().BeFalse();
    }

    [Fact]
    public async Task CopyFileAsync_CopiesFileToDestination()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "source.txt");
        var destDir = Path.Combine(_testDirectory, "dest");
        var destFile = Path.Combine(destDir, "source.txt");

        File.WriteAllText(sourceFile, "test content");
        Directory.CreateDirectory(destDir);

        // Act
        await _sut.CopyFileAsync(sourceFile, destFile);

        // Assert
        File.Exists(destFile).Should().BeTrue();
        File.Exists(sourceFile).Should().BeTrue();
    }

    [Fact]
    public void GetUniqueFilename_WithExistingFile_ReturnsModifiedName()
    {
        // Arrange
        var sourceFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(sourceFile, "test");

        // Act
        var uniqueName = _sut.GetUniqueFilename(_testDirectory, "test.txt");

        // Assert
        uniqueName.Should().Contain("test");
        uniqueName.Should().Contain("_");
        uniqueName.Should().NotBe("test.txt");
    }

    [Fact]
    public void GetUniqueFilename_WithNewFile_ReturnsOriginalName()
    {
        // Act
        var uniqueName = _sut.GetUniqueFilename(_testDirectory, "newfile.txt");

        // Assert
        uniqueName.Should().Be("newfile.txt");
    }

    [Fact]
    public async Task ReadFileAsync_ReadsFileContent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.txt");
        var expectedContent = "test content";
        File.WriteAllText(testFile, expectedContent);

        // Act
        var content = await _sut.ReadFileAsync(testFile);

        // Assert
        System.Text.Encoding.UTF8.GetString(content).Should().Be(expectedContent);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // Silently ignore cleanup errors
        }
    }
}
