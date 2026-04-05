namespace PhotoMover.Tests.Services;

using PhotoMover.Core.Models;
using PhotoMover.Core.Services;
using PhotoMover.Infrastructure.Services;
using Moq;
using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the file-processing logic inside EmbeddedFtpServer.
/// These tests target OnFileUploadCompleted, which is internal and exposed
/// to this assembly via InternalsVisibleTo in PhotoMover.Infrastructure.
/// </summary>
public sealed class EmbeddedFtpServerTests : IDisposable
{
    private readonly Mock<IImportPipeline> _importPipelineMock;
    private readonly Mock<IRuleRepository> _ruleRepositoryMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly EmbeddedFtpServer _sut;
    private readonly List<string> _tempFilesToDelete = new();

    public EmbeddedFtpServerTests()
    {
        _importPipelineMock = new Mock<IImportPipeline>(MockBehavior.Strict);
        _ruleRepositoryMock = new Mock<IRuleRepository>(MockBehavior.Strict);
        _fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

        _sut = new EmbeddedFtpServer(
            _importPipelineMock.Object,
            _ruleRepositoryMock.Object,
            _fileSystemMock.Object);
    }

    // -------------------------------------------------------------------------
    // Guard: temp file does not exist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFileUploadCompleted_WhenTempFileDoesNotExist_DoesNotFireEvent()
    {
        // Arrange
        const string tempFilePath = "/tmp/photo.jpg";
        const string fileName = "photo.jpg";

        _fileSystemMock
            .Setup(x => x.FileExists(tempFilePath))
            .Returns(false);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured.Should().BeNull("no event should fire when the temp file is absent");
        _ruleRepositoryMock.VerifyNoOtherCalls();
        _importPipelineMock.VerifyNoOtherCalls();
    }

    // -------------------------------------------------------------------------
    // Guard: no active grouping rule
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFileUploadCompleted_WhenNoActiveRule_DoesNotFireEvent()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";

        _fileSystemMock
            .Setup(x => x.FileExists(tempFilePath))
            .Returns(true);

        _ruleRepositoryMock
            .Setup(x => x.GetActiveRuleAsync())
            .ReturnsAsync((GroupingRule?)null);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured.Should().BeNull("no event should fire when there is no active rule");
        _importPipelineMock.VerifyNoOtherCalls();
    }

    // -------------------------------------------------------------------------
    // Pipeline success
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineSucceeds_FiresEventWithFinalDestinationPath()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";
        const string destinationPath = "/photos/Sony/2024/01/photo.jpg";

        GroupingRule rule = BuildRule(destinationPath: "/photos");
        ImportResult result = BuildSuccessResult(tempFilePath, destinationPath);

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured.Should().NotBeNull();
        captured!.FilePath.Should().Be(destinationPath);
        captured.FileName.Should().Be("photo.jpg");
        captured.FileSize.Should().BeGreaterThan(0, "size should reflect the temp file bytes");
        captured.ImportResult.Should().Be(result);
        captured.ImportResult!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineSucceeds_FinalFileNameReflectsDestinationName()
    {
        // Arrange – pipeline renamed the file to resolve a conflict
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";
        const string renamedDestination = "/photos/Sony/2024/01/photo_1.jpg";

        GroupingRule rule = BuildRule(destinationPath: "/photos");
        ImportResult result = BuildSuccessResult(tempFilePath, renamedDestination);

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured!.FileName.Should().Be("photo_1.jpg",
            "FileName should reflect the unique name chosen by the pipeline");
    }

    // -------------------------------------------------------------------------
    // Pipeline failure
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineFails_FiresEventWithOriginalTempPath()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";

        GroupingRule rule = BuildRule(destinationPath: "/photos");
        ImportResult result = BuildFailureResult(tempFilePath, "Metadata extraction failed");

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured.Should().NotBeNull();
        captured!.FilePath.Should().Be(tempFilePath,
            "on failure the temp path is reported because the file was not moved");
        captured.FileName.Should().Be(fileName,
            "on failure the original FTP filename is reported");
        captured.ImportResult.Should().Be(result);
        captured.ImportResult!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineFails_ErrorMessageIsPreserved()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";
        const string expectedError = "Pattern evaluation failed";

        GroupingRule rule = BuildRule(destinationPath: "/photos");
        ImportResult result = BuildFailureResult(tempFilePath, expectedError);

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        FtpFileUploadedEventArgs? captured = null;
        _sut.FileUploaded += (_, args) => captured = args;

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        captured!.ImportResult!.ErrorMessage.Should().Be(expectedError);
    }

    // -------------------------------------------------------------------------
    // Pipeline invocation arguments
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineRuns_PassesTempFilePathAsSource()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";

        GroupingRule rule = BuildRule(destinationPath: "/output");
        ImportResult result = BuildSuccessResult(tempFilePath, "/output/Sony/photo.jpg");

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        _importPipelineMock.Verify(
            x => x.ProcessPhotoAsync(
                tempFilePath,
                rule,
                rule.DestinationPath,
                It.IsAny<IProgress<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnFileUploadCompleted_WhenPipelineRuns_UsesActiveRuleDestinationPath()
    {
        // Arrange
        string tempFilePath = CreateTempFile();
        const string fileName = "photo.jpg";
        const string expectedDestinationRoot = "/custom/root";

        GroupingRule rule = BuildRule(destinationPath: expectedDestinationRoot);
        ImportResult result = BuildSuccessResult(tempFilePath, expectedDestinationRoot + "/Sony/photo.jpg");

        ArrangeFileExists(tempFilePath);
        ArrangeActiveRule(rule);
        ArrangePipeline(tempFilePath, rule, result);

        // Act
        await _sut.OnFileUploadCompleted(fileName, tempFilePath);

        // Assert
        _importPipelineMock.Verify(
            x => x.ProcessPhotoAsync(
                It.IsAny<string>(),
                It.IsAny<GroupingRule>(),
                expectedDestinationRoot,
                It.IsAny<IProgress<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void ArrangeFileExists(string tempFilePath)
    {
        _fileSystemMock
            .Setup(x => x.FileExists(tempFilePath))
            .Returns(true);
    }

    private void ArrangeActiveRule(GroupingRule rule)
    {
        _ruleRepositoryMock
            .Setup(x => x.GetActiveRuleAsync())
            .ReturnsAsync(rule);
    }

    private void ArrangePipeline(string tempFilePath, GroupingRule rule, ImportResult result)
    {
        _importPipelineMock
            .Setup(x => x.ProcessPhotoAsync(
                tempFilePath,
                rule,
                rule.DestinationPath,
                It.IsAny<IProgress<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private string CreateTempFile()
    {
        string path = Path.GetTempFileName();
        File.WriteAllBytes(path, new byte[] { 0xFF, 0xD8, 0xFF }); // minimal JPEG header bytes
        _tempFilesToDelete.Add(path);
        return path;
    }

    private static GroupingRule BuildRule(string destinationPath) => new GroupingRule
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Test Rule",
        PathPattern = "{CameraModel}/{DateTaken:yyyy}",
        IsActive = true,
        Priority = 0,
        DestinationPath = destinationPath,
        Metadata = new Dictionary<string, string>()
    };

    private static ImportResult BuildSuccessResult(string sourcePath, string destinationPath) => new ImportResult
    {
        SourcePath = sourcePath,
        DestinationPath = destinationPath,
        Success = true,
        ErrorMessage = null,
        Metadata = null,
        ProcessedAt = DateTime.Now
    };

    private static ImportResult BuildFailureResult(string sourcePath, string errorMessage) => new ImportResult
    {
        SourcePath = sourcePath,
        DestinationPath = string.Empty,
        Success = false,
        ErrorMessage = errorMessage,
        Metadata = null,
        ProcessedAt = DateTime.Now
    };

    public void Dispose()
    {
        foreach (string path in _tempFilesToDelete)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        _sut.Dispose();
    }
}
