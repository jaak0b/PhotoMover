using JetBrains.Annotations;

namespace Domain.Model
{
  [UsedImplicitly]
  public class TaskModel()
  {
    public string SourceFilePath { get; set; } = null!;

    public string DestinationFilePath { get; set; } = null!;

    public DateTime Created { get; } = DateTime.Now;

    public string? ErrorMessage { get; set; }

    public bool? FileAlreadyExists { get; set; }
  }
}