namespace Domain.UnitTest;

internal static class Domain
{
    internal static DirectoryInfo BaseFolder => new(Path.Combine(Path.GetTempPath(), "PhotoMover"));

    internal static DirectoryInfo SourceFolder => new(Path.Combine(BaseFolder.FullName, "SourceFolder"));

    internal static DirectoryInfo TargetFolder => new(Path.Combine(BaseFolder.FullName, "TargetFolder"));
}