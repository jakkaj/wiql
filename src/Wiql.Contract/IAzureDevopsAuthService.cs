namespace Wiql.Contract
{
    public interface IAzureDevopsAuthService
    {
        string GetBasicAuth(string pat, string userEmail);
        string GetBasicAuth();
        string GetTeamBaseUrl();
        string GetProjectBaseUrl();
        void WriteSettings();
    }
}