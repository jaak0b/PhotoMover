namespace PhotoMover.Tests.Services;

using PhotoMover.Infrastructure.Services;
using Moq;
using Xunit;
using FluentAssertions;
using PhotoMover.Core.Services;
using PhotoMover.Core.Models;

/// <summary>
/// Unit tests for the JsonRuleRepository service.
/// </summary>
public sealed class JsonRuleRepositoryTests : IDisposable
{
    private readonly string _testStoragePath = Path.Combine(Path.GetTempPath(), $"PhotoMoverRulesTest_{Guid.NewGuid()}");
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly JsonRuleRepository _sut;

    public JsonRuleRepositoryTests()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _fileSystemMock.Setup(x => x.CreateDirectory(It.IsAny<string>())).Callback<string>(p =>
        {
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
        });

        _fileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns<string>(File.Exists);
        _fileSystemMock.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((p, s) =>
            Directory.Exists(p) ? Directory.GetFiles(p, s) : new string[0]);

        Directory.CreateDirectory(_testStoragePath);
        _sut = new JsonRuleRepository(_fileSystemMock.Object, _testStoragePath);
    }

    [Fact]
    public async Task SaveRuleAsync_SavesRuleToFile()
    {
        // Arrange
        var rule = new GroupingRule
        {
            Id = "test-rule",
            Name = "Test Rule",
            PathPattern = "{CameraModel}/{DateTaken:yyyy}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act
        await _sut.SaveRuleAsync(rule);

        // Assert
        var filePath = Path.Combine(_testStoragePath, "test-rule.json");
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task GetRuleByIdAsync_RetrievesRule()
    {
        // Arrange
        var rule = new GroupingRule
        {
            Id = "test-rule",
            Name = "Test Rule",
            PathPattern = "{CameraModel}/{DateTaken:yyyy}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };

        await _sut.SaveRuleAsync(rule);

        // Act
        var retrieved = await _sut.GetRuleByIdAsync("test-rule");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Rule");
        retrieved.PathPattern.Should().Be("{CameraModel}/{DateTaken:yyyy}");
    }

    [Fact]
    public async Task GetAllRulesAsync_ReturnsAllSavedRules()
    {
        // Arrange
        var rule1 = new GroupingRule
        {
            Id = "rule1",
            Name = "Rule 1",
            PathPattern = "{CameraModel}",
            IsActive = false,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };

        var rule2 = new GroupingRule
        {
            Id = "rule2",
            Name = "Rule 2",
            PathPattern = "{DateTaken:yyyy}",
            IsActive = false,
            Priority = 1,
            Metadata = new Dictionary<string, string>()
        };

        await _sut.SaveRuleAsync(rule1);
        await _sut.SaveRuleAsync(rule2);

        // Act
        var rules = await _sut.GetAllRulesAsync();

        // Assert
        rules.Should().HaveCount(2);
        rules.Should().Contain(r => r.Id == "rule1");
        rules.Should().Contain(r => r.Id == "rule2");
    }

    [Fact]
    public async Task DeleteRuleAsync_RemovesRule()
    {
        // Arrange
        var rule = new GroupingRule
        {
            Id = "test-rule",
            Name = "Test",
            PathPattern = "{CameraModel}",
            IsActive = false,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };

        await _sut.SaveRuleAsync(rule);
        var filePath = Path.Combine(_testStoragePath, "test-rule.json");
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _sut.DeleteRuleAsync("test-rule");

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task SetActiveRuleAsync_StoresActiveRuleId()
    {
        // Arrange
        var rule = new GroupingRule
        {
            Id = "active-rule",
            Name = "Active",
            PathPattern = "{CameraModel}",
            IsActive = true,
            Priority = 0,
            Metadata = new Dictionary<string, string>()
        };

        await _sut.SaveRuleAsync(rule);

        // Act
        await _sut.SetActiveRuleAsync("active-rule");

        // Assert
        var activeFilePath = Path.Combine(_testStoragePath, "active.txt");
        var content = File.ReadAllText(activeFilePath).Trim();
        content.Should().Be("active-rule");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testStoragePath))
            {
                Directory.Delete(_testStoragePath, recursive: true);
            }
        }
        catch
        {
            // Silently ignore cleanup errors
        }
    }
}
