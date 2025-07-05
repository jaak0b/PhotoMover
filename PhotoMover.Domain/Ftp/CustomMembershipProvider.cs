using FubarDev.FtpServer.AccountManagement;

namespace Domain.Ftp
{
  public class PhotoMoverMembershipProvider(IAppConfig appConfig) : IMembershipProvider
  {
    public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
    {
      if (username == appConfig.FtpConfig.Credentials?.UserName &&
          password == appConfig.FtpConfig.Credentials?.Password)
      {
        return Task.FromResult(
                               new MemberValidationResult(
                                                          MemberValidationStatus.AuthenticatedUser,
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