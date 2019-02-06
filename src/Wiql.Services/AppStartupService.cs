using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Wiql.Contract;
using Wiql.Model.Model;

namespace Wiql.Services
{
    public class AppStartupService : IAppStartupService
    {
        private readonly IAzureDevOpsService _azureDevOpsService;
        private readonly IAzureDevopsAuthService _authService;
        private readonly IOptions<AzureDevOpsSettings> _options;

        public AppStartupService(IAzureDevOpsService azureDevOpsService, 
            IAzureDevopsAuthService authService,
            IOptions<AzureDevOpsSettings> options)
        {
            _azureDevOpsService = azureDevOpsService;
            _authService = authService;
            _options = options;
        }
        public async Task<string> RunApp()
        {
            if (!_validate())
            {
                return "2";
            }
            //_authService.WriteSettings();
            var result = await _azureDevOpsService.RunQuery(_options.Value.WiqlQuery);
            var json = JsonConvert.SerializeObject(result);

            return json;
        }

        bool _validate()
        {
            var opts = _options.Value;

            if (!_check(opts.PersonalAccessToken, "Please pass a personal access token."))
            {
                return false;
            }

            if (!_check(opts.Organization, "Please pass an Azure DevOps Organization"))
            {
                return false;
            }

            if (!_check(opts.Project, "Please pass an Azure DevOps Project"))
            {
                return false;
            }

            if (!_check(opts.Team, "Please pass an Azure DevOps Team"))
            {
                return false;
            }

            if (!_check(opts.UserEmail, "Please pass an Azure DevOps User Email address for this PAT"))
            {
                return false;
            }

            if (!_check(opts.WiqlQuery, "Please pass a WIQL query by --query or pipe it"))
            {
                return false;
            }

            return true;

        }

        bool _check(string val, string errorText)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                Console.WriteLine($"Please check inputs: {errorText}");
                return false;
            }

            return true;
        }
    }
}
