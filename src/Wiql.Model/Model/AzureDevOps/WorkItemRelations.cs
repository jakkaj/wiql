using System;
using System.Collections.Generic;
using System.Text;

namespace Wiql.Model.Model.AzureDevOps
{
    public class WorkItemRelations
    {
        public List<WorkItemData> WorkItems { get; set; }
        public WorkItemRelatedResults Relations { get; set; }
    }
}
