namespace Wiql.Contract
{
    public interface IAzureDevopsAuthService
    {
        string GetBasicAuth(string pat, string userEmail);
    }
}