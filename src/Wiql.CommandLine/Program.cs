﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wiql.Contract;

[assembly: InternalsVisibleTo("Wiql.Tests")]

namespace Wiql.CommandLine
{
    
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pat">Azure DevOps Personal Access token - see https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops </param>
        /// <param name="userEmail">Email address of Azure DevOps user</param>
        /// <param name="org">The organization from Azure DevOps</param>
        /// <param name="project">The project from Azure DevOps</param>
        /// <param name="team">The team from Azure DevOps</param>
        /// <param name="query">A WIQL query. Do not use with teh workItems flag</param>
        /// <param name="workItems">A comma separated list of work item ids to load</param>
        /// <returns>Json result</returns>
        public static async Task Main(string pat = null, 
            string userEmail = null, 
            string org = null, 
            string project = null, 
            string team = null, 
            string query = null
            )
        {
            if (!string.IsNullOrWhiteSpace(pat))
            {
                _setVar("PersonalAccessToken", pat);
            }

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                _setVar("UserEmail", userEmail);
            }

            if (!string.IsNullOrWhiteSpace(org))
            {
                _setVar("Organization", org);
            }

            if (!string.IsNullOrWhiteSpace(project))
            {
                _setVar("Project", project);
            }

            if (!string.IsNullOrWhiteSpace(team))
            {
                _setVar("Team", team);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                _setVar("WiqlQuery", query);
            }
           
            var pipedQuery = "";

            var isKeyAvailable = false;

            try
            {
                isKeyAvailable = System.Console.KeyAvailable;
            }

            catch (InvalidOperationException expected)
            {
                pipedQuery = System.Console.In.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(pipedQuery))
            {
                //Check for strangeness piped in from console
                var bytes = Encoding.UTF8.GetBytes(pipedQuery);                
                if(bytes[0] == 239){
                    pipedQuery = pipedQuery.Substring(1);
                }
                _setVar("WiqlQuery", pipedQuery);
            }

            var appHost = new AppHostBase();

            var appStartup = appHost.Resolve<IAppStartupService>();
            
            var result = await appStartup.RunApp();
            int n;
            bool isNumeric = int.TryParse(result, out n);
            if (isNumeric)
            {
                Environment.Exit(n);
                return;
            }
            Console.WriteLine(result);
            
        }

        static void _setVar(string var, string value)
        {
            System.Environment.SetEnvironmentVariable($"AzureDevOpsSettings__{var}", value);
        }
    }
}
