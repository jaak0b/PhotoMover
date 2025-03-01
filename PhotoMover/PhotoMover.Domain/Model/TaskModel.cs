using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Domain.Model;

[UsedImplicitly]
public class TaskModel()
{
    public int Id { get; set; }
    
    [Required]
    public ConfigurationModel Configuration { get; set; } = default!;

    [Required]
    [StringLength(32000)]
    public string SourceFilePath { get; } = default!;

    [StringLength(32000)]
    public string? DestinationFilePath { get; set; } = default!;

    public DateTime Created { get; } = DateTime.Now;

    public TaskType Type { get; set; }

    public State State { get; set; } = State.Created;

    #region helper methods

    public FileInfo SourceFile => new(SourceFilePath);

    #endregion
}