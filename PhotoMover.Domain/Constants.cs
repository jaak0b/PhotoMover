namespace Domain
{
  public static class Constants
  {
    public const string AppName = "PhotoMover";

    public static Lazy<string> ApplicationDataPath => new(ApplicationDataPathValueFactory);

    public static Lazy<string> SqlLiteFilePath => new(() => Path.Combine(ApplicationDataPath.Value, "db.sqlite"));

    public static Lazy<string> AppConfigFilePath => new(() => Path.Combine(ApplicationDataPath.Value, "AppConfig.ini"));

    private static string ApplicationDataPathValueFactory()
    {
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
      Directory.CreateDirectory(path);
      return path;
    }
  }
}