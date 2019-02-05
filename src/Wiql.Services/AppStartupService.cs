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
            _authService.WriteSettings();
            var result = await _azureDevOpsService.RunQuery(_options.Value.WiqlQuery);
            var json = JsonConvert.SerializeObject(result);

            return json;
        }
    }
}
