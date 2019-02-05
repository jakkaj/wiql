using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wiql.Services.ServiceSetup
{
    public class AppHost<THostAppClass> where THostAppClass : class
    {
        private IServiceProvider ServiceProvider { get; set; }
        private IServiceCollection ServiceCollection { get; set; }

        public void Boot()
        {
            DotNetEnv.Env.Load(true, true, true, false);

            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            builder.AddUserSecrets<THostAppClass>();

            var Configuration = builder.Build();


            ServiceCollection = new ServiceCollection();

            ServiceCollection.AddCoreServices(Configuration);


            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }

        public T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }
    }
}
