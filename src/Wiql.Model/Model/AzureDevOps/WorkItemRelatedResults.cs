using System;
using System.Collections.Generic;
using System.Text;

namespace Wiql.Model.Model.AzureDevOps
{
    public class WColumn
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class WField
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class WSortColumn
    {
        public WField field { get; set; }
        public bool descending { get; set; }
    }

    public class Source
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Target
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class WorkItemRelation
    {
        public string rel { get; set; }
        public Source source { get; set; }
        public Target target { get; set; }
    }

    public class WorkItemRelatedResults
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        public DateTime asOf { get; set; }
        public List<WColumn> columns { get; set; }
        public List<WSortColumn> sortColumns { get; set; }
        public List<WorkItemRelation> workItemRelations { get; set; }
    }
}
