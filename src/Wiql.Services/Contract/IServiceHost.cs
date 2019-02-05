using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wiql.Services.ServiceSetup;

namespace Wiql.Services.Contract
{
    public interface IServiceHost
    {
        IServiceCollection Services { get; }
        IServiceProvider ServiceProvider { get; }
        ServiceHost Configure(IServiceCollection services, IConfiguration configuration);
        IServiceProvider Build();
    }
}