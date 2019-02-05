using System;
using System.Collections.Generic;
using System.Text;

namespace Wiql.Model.Model.AzureDevOps
{
    public class WorkItemGet
    {
        public string count { get; set; }
        public List<WorkItemData> value { get; set; }
    }
}
