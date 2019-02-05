using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using Wiql.Contract;
using Wiql.Model.Model;

namespace Wiql.Services
{
    public class AzureDevopsAuthService : IAzureDevopsAuthService
    {
        private readonly IOptions<AzureDevOpsSettings> _options;

        public AzureDevopsAuthService(IOptions<AzureDevOpsSettings> options)
        {
            _options = options;
        }

        public void WriteSettings()
        {
            Console.WriteLine($"{_options.Value.Organization}, {_options.Value.Project}, {_options.Value.Team}");
        }
        

        public string GetProjectBaseUrl()
        {
            var org = HttpUtility.UrlEncode(_options.Value.Organization);
            var proj = HttpUtility.UrlEncode(_options.Value.Project);
            var url = $"https://dev.azure.com/{org}/{proj}/";

            return url;
        }
        
        public string GetTeamBaseUrl()
        {
            var org = HttpUtility.HtmlEncode(_options.Value.Organization);
            var proj = HttpUtility.HtmlEncode(_options.Value.Project);
            var team = HttpUtility.HtmlEncode(_options.Value.Team);
            var url = $"https://dev.azure.com/{org}/{proj}/{team}/";

            return url;
        }
        public string GetBasicAuth()
        {
            var pat = _options.Value.PersonalAccessToken;
            var email = _options.Value.UserEmail;

            return GetBasicAuth(pat, email);
        }
        public string GetBasicAuth(string pat, string userEmail)
        {
            var bytes = Encoding.UTF8.GetBytes($"{userEmail}:{pat}");

            var b64 = Convert.ToBase64String(bytes);

            var result = $"Basic {b64}";

            return result;
        }
    }
}
