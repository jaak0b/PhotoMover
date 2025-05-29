using Domain.Model;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Domain.Service
{
  public interface IFtpConfigurationService
  {
    public FtpPresetModel GetFtpConfigurationModel();
  }

  public class FtpPresetService(Database db) : IFtpConfigurationService
  {
    private Database Db { get; } = db;

    public FtpPresetModel GetFtpConfigurationModel()
    {
      FtpPresetModel? ftp = Db.FtpConfiguration;
      if (ftp == null)
      {
        ftp = new();
        Db.Add(ftp);
      }

      return ftp;
    }
  }
}