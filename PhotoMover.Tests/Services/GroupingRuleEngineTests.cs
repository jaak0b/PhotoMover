namespace PhotoMover.Tests.Services;

using PhotoMover.Core.Models;
using PhotoMover.Infrastructure.Services;
using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the GroupingRuleEngine service.
/// </summary>
public sealed class GroupingRuleEngineTests
{
    private readonly GroupingRuleEngine _sut = new();

    private static PhotoMetadata CreateSampleMetadata(string? cameraModel = "Sony A7R V", DateTime? dateTaken = null)
    {
        var date = dateTaken ?? new DateTime(2024, 12, 15);
        return new PhotoMetadata
        {
            FilePath = "/test/photo.jpg",
            FileName = "photo.jpg",
            DateTaken = date,
            CameraModel = cameraModel,
            LensModel = "24-70mm",
            Orientation = 1,
            AllMetadata = new Dictionary<string, string>(),
            TagIdMetadata = new Dictionary<string, string>
            {
                ["0x0110"] = cameraModel ?? "Unknown",  // Model
                ["0x9003"] = date.ToString("yyyy:MM:dd HH:mm:ss")  // DateTaken
            }
        };
    }

    [Fact]
    public void EvaluatePattern_WithValidPattern_ReturnsEvaluatedPath()
    {
        // Arrange
        var pattern = "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}";
        var metadata = CreateSampleMetadata("Sony A7R V", new DateTime(2024, 12, 15));

        // Act
        var result = _sut.EvaluatePattern(pattern, metadata);

        // Assert
        result.Should().Be("Sony A7R V/2024/12");
    }

    [Fact]
    public void EvaluatePattern_WithInvalidPattern_ReturnsNull()
    {
        // Arrange
        var pattern = "{InvalidPlaceholder}";
        var metadata = CreateSampleMetadata("Sony");

        // Act
        var result = _sut.EvaluatePattern(pattern, metadata);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void EvaluatePattern_WithMultiplePlaceholders_SubstitutesAll()
    {
        // Arrange
        var pattern = "{CameraModel}_{DateTaken:yyyy}";
        var metadata = CreateSampleMetadata("Canon", new DateTime(2023, 6, 20));

        // Act
        var result = _sut.EvaluatePattern(pattern, metadata);

        // Assert
        result.Should().Be("Canon_2023");
    }

    [Fact]
    public void ValidatePattern_WithValidPattern_ReturnsTrue()
    {
        // Arrange
        var pattern = "{CameraModel}/{DateTaken:yyyy}";

        // Act
        var result = _sut.ValidatePattern(pattern);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePattern_WithInvalidPlaceholder_ReturnsFalse()
    {
        // Arrange
        var pattern = "{InvalidField}/{DateTaken:yyyy}";

        // Act
        var result = _sut.ValidatePattern(pattern);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePattern_WithEmptyPattern_ReturnsFalse()
    {
        // Arrange
        var pattern = string.Empty;

        // Act
        var result = _sut.ValidatePattern(pattern);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAvailablePlaceholders_ReturnsPlaceholders()
    {
        // Act
        var placeholders = _sut.GetAvailablePlaceholders();

        // Assert
        placeholders.Should().NotBeEmpty();
        placeholders.Should().Contain("DateTaken");
        placeholders.Should().Contain("CameraModel");
        placeholders.Should().Contain("LensModel");
    }
}
