namespace Domain.UnitTest
{
  internal static class Domain
  {
    internal static DirectoryInfo BaseFolder => new(Path.Combine(Path.GetTempPath(), "PhotoMover"));

    internal static DirectoryInfo SourceFolder => new(Path.Combine(BaseFolder.FullName, "SourceFolder"));

    internal static DirectoryInfo TargetFolder => new(Path.Combine(BaseFolder.FullName, "TargetFolder"));


    public static class TestData
    {
      internal static DirectoryInfo Folder => new(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData"));

      public const int FileCount = 2;

      public const int JpgCount = 1;
    }
  }
}