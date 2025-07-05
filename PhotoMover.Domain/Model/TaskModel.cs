using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Domain.Model
{
  [UsedImplicitly]
  public class TaskModel()
  {
    public int Id { get; set; }

    [Required]
    [StringLength(32000)]
    public string SourceFile { get; set; } = default!;

    [StringLength(32000)]
    public string? DestinationFile { get; set; } = default!;

    public DateTime Created { get; } = DateTime.Now;

    public TaskType Type { get; set; }

    public State State { get; set; } = State.Created;
  }
}