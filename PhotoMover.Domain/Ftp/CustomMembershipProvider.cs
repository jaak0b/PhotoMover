using Domain.Model;
using Domain.Service;
using FubarDev.FtpServer.AccountManagement;

namespace Domain.Ftp;

public class PhotoMoverMembershipProvider(FtpPresetService ftpPresetService) : IMembershipProvider
{
    public FtpPresetService FtpPresetService { get; } = ftpPresetService;

    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
        FtpPresetModel config = FtpPresetService.GetFtpConfigurationModel();
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