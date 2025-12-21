using FubarDev.FtpServer.AccountManagement;

namespace Domain.Ftp
{
  public class PhotoMoverMembershipProvider(ISettingsProvider provider) : IMembershipProvider
  {
    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
      if (username == provider.Settings.Value.Credentials?.UserName && password == provider.Settings.Value.Credentials?.Password)
      {
        return Task.FromResult(new MemberValidationResult(MemberValidationStatus.AuthenticatedUser,
                                                          new User(username)));
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
}