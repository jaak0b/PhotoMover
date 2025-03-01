using System.ComponentModel.DataAnnotations;

namespace Domain.Model;

public class ConfigurationModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(32000)]
    public string SourceFolder { get; set; } = default!;

    [Required]
    [StringLength(32000)]
    public string DestinationFolderPath { get; set; } = default!;
}