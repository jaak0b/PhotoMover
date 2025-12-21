using System.Net;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Domain
{
  public class AppSettings
  {
    public string FilePattern { get; set; } = "*";

    public string SourceFolder { get; set; }

    public string DestinationFolder { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public List<GroupOptions> Groups { get; set; } = [];

    public bool FtpIsActive { get; set; } = false;

    public string FtpServerIpAddress { get; set; } = "127.0.0.1";

    public NetworkCredential Credentials { get; set; } = new();
  }

  public enum GroupOptions
  {
    FileCreated,
    CameraTaken
  }
}