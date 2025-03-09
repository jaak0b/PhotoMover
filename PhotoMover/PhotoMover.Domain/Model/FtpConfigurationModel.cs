using System.ComponentModel.DataAnnotations;

namespace Domain.Model;

public class FtpConfigurationModel : ConfigurationModel
{
    public FtpConfigurationModel()
    {
        
    }
    
    public bool IsActive { get; set; }

    [StringLength(40)]
    public string FtpServerIpAddress { get; set; } = "127.0.0.1";

    [StringLength(1000)]
    public string FtpUserName { get; set; } = "";

    [StringLength(1000)]
    public string FtpPassword { get; set; } = "";
}