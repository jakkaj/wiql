using System;
using Wiql.Services.Contract;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wiql.Contract;
using Wiql.Model.Model;

namespace Wiql.Services.ServiceSetup
{
    public class ServiceHost : IServiceHost
    {
        public IServiceCollection Services { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }


        public ServiceHost Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureDevOpsSettings>(configuration.GetSection(nameof(AzureDevOpsSettings)));
            services.AddTransient<IAzureDevopsAuthService, AzureDevopsAuthService>();

            return this;
        }

        public IServiceProvider Build()
        {
            ServiceProvider = Services.BuildServiceProvider();
            return ServiceProvider;
        }
    }
}
