using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wiql.Model.Model.AzureDevOps;

namespace Wiql.Contract
{
    public interface IAzureDevOpsService
    {
        Task<List<SimpleWorkItem>> RunQuery(string query);
        string RunQueryRaw(string query);
    }
}
