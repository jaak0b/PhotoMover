using System.IO;
using Autofac.Extensions.DependencyInjection;
using Domain;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace PhotoMover
{
  [UsedImplicitly]
  public class PhotoMoverPhotoMoverServiceProvider : PhotoMoverServiceProvider
  {
    private static string FullPath =>
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PhotoMover", "Data");

    private static string FullFilePath =>
      Path.Combine(FullPath, "db.sqlite");

    protected override void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite($"Data Source={FullFilePath}");
    }

    protected override void CreateDatabase(AutofacServiceProvider serviceProvider)
    {
      if (!Directory.Exists(FullPath))
      {
        Directory.CreateDirectory(FullPath);
      }

      base.CreateDatabase(serviceProvider);
    }
  }
}