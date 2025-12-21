using System.Text.Json;

namespace Domain
{
  public interface ISettingsProvider
  {
    public Lazy<AppSettings> Settings { get; }

    string SettingsFolder { get; }
    void Save();
  }

  public class SettingsProvider : ISettingsProvider
  {
    public SettingsProvider()
    {
      Settings = new(SettingsFactory);
    }

    private AppSettings SettingsFactory()
    {
      if (!File.Exists(SettingsFilePath))
        return new();
      string json = File.ReadAllText(SettingsFilePath);
      return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public Lazy<AppSettings> Settings { get; }

    public string SettingsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhotoMover");

    public string SettingsFilePath => Path.Combine(SettingsFolder, "AppSettings.json");

    public void Save()
    {
      if (!Directory.Exists(SettingsFolder))
        Directory.CreateDirectory(SettingsFolder);

      if (!File.Exists(SettingsFilePath))
        File.Create(SettingsFilePath);

      string json = JsonSerializer.Serialize(Settings.Value, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(SettingsFilePath, json);
    }
  }
}