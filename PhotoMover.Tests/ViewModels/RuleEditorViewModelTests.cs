namespace PhotoMover.Tests.ViewModels;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using PhotoMover.ViewModels;
using Moq;
using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the RuleEditorViewModel.
/// </summary>
public sealed class RuleEditorViewModelTests
{
    private readonly Mock<IGroupingRuleEngine> _groupingRuleEngineMock;
    private readonly Mock<IRuleRepository> _ruleRepositoryMock;
    private readonly Mock<IMetadataExtractor> _metadataExtractorMock;
    private readonly RuleEditorViewModel _sut;

    public RuleEditorViewModelTests()
    {
        _groupingRuleEngineMock = new Mock<IGroupingRuleEngine>();
        _ruleRepositoryMock = new Mock<IRuleRepository>();
        _metadataExtractorMock = new Mock<IMetadataExtractor>();

        _groupingRuleEngineMock
            .Setup(x => x.GetAvailablePlaceholders())
            .Returns(new[] { "DateTaken", "CameraModel", "LensModel" });

        _groupingRuleEngineMock
            .Setup(x => x.ValidatePattern(It.IsAny<string>()))
            .Returns(true);

        _groupingRuleEngineMock
            .Setup(x => x.EvaluatePattern(It.IsAny<string>(), It.IsAny<PhotoMetadata>()))
            .Returns("Sony/2024/12");

        _sut = new RuleEditorViewModel(
            _groupingRuleEngineMock.Object,
            _ruleRepositoryMock.Object,
            _metadataExtractorMock.Object);
    }

    [Fact]
    public void RuleName_CanBeSet()
    {
        // Act
        _sut.RuleName = "My Custom Rule";

        // Assert
        _sut.RuleName.Should().Be("My Custom Rule");
    }

    [Fact]
    public void PathPattern_CanBeSet()
    {
        // Act
        _sut.PathPattern = "{CameraModel}/{DateTaken:yyyy}";

        // Assert
        _sut.PathPattern.Should().Be("{CameraModel}/{DateTaken:yyyy}");
    }

    [Fact]
    public void PathPattern_WhenChanged_UpdatesPreview()
    {
        // Act
        _sut.PathPattern = "{CameraModel}/{DateTaken:yyyy}";

        // Assert
        _sut.PreviewPath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SaveRuleAsync_CallsRepository()
    {
        // Act
        await _sut.SaveRuleAsync();

        // Assert
        _ruleRepositoryMock.Verify(x => x.SaveRuleAsync(It.IsAny<GroupingRule>()), Times.Once);
    }

    [Fact]
    public void AvailablePlaceholders_ReturnsPlaceholders()
    {
        // Act
        var placeholders = _sut.AvailablePlaceholders;

        // Assert
        placeholders.Should().NotBeEmpty();
        placeholders.Should().Contain("DateTaken");
    }

    [Fact]
    public void InsertPlaceholder_AppendsPlaceholderToPattern()
    {
        // Arrange
        var originalPattern = _sut.PathPattern;

        // Act
        _sut.InsertPlaceholder("CameraModel");

        // Assert
        _sut.PathPattern.Should().Contain("{CameraModel}");
    }
}
