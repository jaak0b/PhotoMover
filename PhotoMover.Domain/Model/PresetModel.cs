using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
  public class PresetModel
  {
    public int Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; } = "New preset!";

    [StringLength(32000)]
    public string? SourceFolder { get; set; } = default!;

    [StringLength(32000)]
    public string? DestinationFolder { get; set; } = default!;

    [StringLength(32000)]
    public string FilePattern { get; set; } = "*";

    [StringLength(32000)]
    public string? FolderPattern { get; set; } = default!;

    public DirectoryInfo? GetSourceFolder()
    {
      return Directory.Exists(SourceFolder) ? new DirectoryInfo(SourceFolder) : null;
    }

    public DirectoryInfo? GetDestinationFolder()
    {
      return Directory.Exists(DestinationFolder) ? new DirectoryInfo(DestinationFolder) : null;
    }
  }
}