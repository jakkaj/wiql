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
        private readonly IOptions<AzureDevOpsSettings> _options;

        public AppStartupService(IAzureDevOpsService azureDevOpsService, 
            IOptions<AzureDevOpsSettings> options)
        {
            _azureDevOpsService = azureDevOpsService;
            _options = options;
        }
        public async Task<string> RunApp()
        {
            var result = await _azureDevOpsService.RunQuery(_options.Value.WiqlQuery);
            var json = JsonConvert.SerializeObject(result);

            return json;
        }
    }
}
