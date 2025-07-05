using System.ComponentModel;
using System.Net;
using Config.Net;

namespace Domain
{
  public interface IAppConfig : INotifyPropertyChanged
  {
    public string FolderSource { get; set; }

    public string FolderTarget { get; set; }

    [Option(DefaultValue = "*")]
    public string FilePattern { get; set; }

    public string FolderPattern { get; set; }

    public IFtpConfig FtpConfig { get; set; }
  }

  public interface IFtpConfig : INotifyPropertyChanged
  {
    [Option(DefaultValue = false)]
    public bool IsActive { get; set; }

    [Option(DefaultValue = "127.0.0.1")]
    public string FtpServerIpAddress { get; set; }
    
    public string FolderSource { get; set; }

    public NetworkCredential Credentials { get; set; }
  }
}