namespace PhotoMover.Tests.Services;

using PhotoMover.Core.Models;
using PhotoMover.Infrastructure.Services;
using Moq;
using Xunit;
using FluentAssertions;
using PhotoMover.Core.Services;

/// <summary>
/// Unit tests for the ImportPipeline service.
/// </summary>
public sealed class ImportPipelineTests
{
    private readonly Mock<IMetadataExtractor> _metadataExtractorMock;
    private readonly Mock<IGroupingRuleEngine> _groupingRuleEngineMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly ImportPipeline _sut;

    public ImportPipelineTests()
    {
        _metadataExtractorMock = new Mock<IMetadataExtractor>();
        _groupingRuleEngineMock = new Mock<IGroupingRuleEngine>();
        _fileSystemMock = new Mock<IFileSystem>();

        _sut = new ImportPipeline(
            _metadataExtractorMock.Object,
            _groupingRuleEngineMock.Object,
            _fileSystemMock.Object);
    }

    [Fact]
    public async Task ProcessPhotoAsync_WithValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var sourcePath = "/source/photo.jpg";
        var rule = new GroupingRule
        {
            Id = "1",
            Name = "Test",
            PathPattern = "{CameraModel}/{DateTaken:yyyy}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };
        var destinationRoot = "/destination";

        var metadata = new PhotoMetadata
        {
            FilePath = sourcePath,
            FileName = "photo.jpg",
            DateTaken = new DateTime(2024, 12, 15),
            CameraModel = "Sony",
            LensModel = null,
            Orientation = 1,
            AllMetadata = new Dictionary<string, string>(),
            TagIdMetadata = new Dictionary<string, string>
            {
                ["0x0110"] = "Sony",
                ["0x9003"] = "2024:12:15 12:00:00"
            }
        };

        _metadataExtractorMock
            .Setup(x => x.ExtractMetadataAsync(sourcePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);

        _groupingRuleEngineMock
            .Setup(x => x.EvaluatePattern(rule.PathPattern, metadata))
            .Returns("Sony/2024");

        _fileSystemMock
            .Setup(x => x.GetUniqueFilename(It.IsAny<string>(), "photo.jpg"))
            .Returns("photo.jpg");

        // Act
        var result = await _sut.ProcessPhotoAsync(sourcePath, rule, destinationRoot);

        // Assert
        result.Success.Should().BeTrue();
        result.Metadata.Should().Be(metadata);
        result.SourcePath.Should().Be(sourcePath);
    }

    [Fact]
    public async Task ProcessPhotoAsync_WhenMetadataExtractionFails_ReturnsFailureResult()
    {
        // Arrange
        var sourcePath = "/source/photo.jpg";
        var rule = new GroupingRule
        {
            Id = "1",
            Name = "Test",
            PathPattern = "{CameraModel}/{DateTaken:yyyy}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };
        var destinationRoot = "/destination";

        _metadataExtractorMock
            .Setup(x => x.ExtractMetadataAsync(sourcePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PhotoMetadata?)null);

        // Act
        var result = await _sut.ProcessPhotoAsync(sourcePath, rule, destinationRoot);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessPhotosAsync_WithMultipleFiles_ProcessesAll()
    {
        // Arrange
        var files = new[] { "/source/photo1.jpg", "/source/photo2.jpg" };
        var rule = new GroupingRule
        {
            Id = "1",
            Name = "Test",
            PathPattern = "{CameraModel}/{DateTaken:yyyy}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };
        var destinationRoot = "/destination";

        var metadata = new PhotoMetadata
        {
            FilePath = "/source/photo.jpg",
            FileName = "photo.jpg",
            DateTaken = new DateTime(2024, 12, 15),
            CameraModel = "Sony",
            LensModel = null,
            Orientation = 1,
            AllMetadata = new Dictionary<string, string>(),
            TagIdMetadata = new Dictionary<string, string>
            {
                ["0x0110"] = "Sony",
                ["0x9003"] = "2024:12:15 12:00:00"
            }
        };

        _metadataExtractorMock
            .Setup(x => x.ExtractMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);

        _groupingRuleEngineMock
            .Setup(x => x.EvaluatePattern(It.IsAny<string>(), It.IsAny<PhotoMetadata>()))
            .Returns("Sony/2024");

        _fileSystemMock
            .Setup(x => x.GetUniqueFilename(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("photo.jpg");

        // Act
        var results = await _sut.ProcessPhotosAsync(files, rule, destinationRoot);

        // Assert
        results.Count.Should().Be(2);
        results.All(r => r.Success).Should().BeTrue();
    }
}
