using Domain.Model;
using Domain.Service;
using FubarDev.FtpServer.AccountManagement;

namespace Domain.Ftp;

public class PhotoMoverMembershipProvider(FtpConfigurationService ftpConfigurationService) : IMembershipProvider
{
    public FtpConfigurationService FtpConfigurationService { get; } = ftpConfigurationService;

    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        FtpConfigurationModel config = FtpConfigurationService.GetFtpConfigurationModel();
        if (username == config.FtpUserName &&
            password == config.FtpPassword)
        {
            return Task.FromResult(
                new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, new User(username)));
        }

        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
    }
}

public class User(string username) : IFtpUser
{
    public string Name { get; } = username;

    public bool IsInGroup(string groupName)
    {
        return true;
    }
}