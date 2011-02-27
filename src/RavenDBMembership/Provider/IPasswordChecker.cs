namespace RavenDBMembership.Provider
{
    public interface IPasswordChecker
    {
        bool CheckPassword(string username, string password, bool updateLastLogin);
    }
}