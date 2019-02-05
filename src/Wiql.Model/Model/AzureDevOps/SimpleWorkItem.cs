using System;
using System.Collections.Generic;
using System.Text;

namespace Wiql.Model.Model.AzureDevOps
{
    public class SimpleWorkItem
    {
        public string Url { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public List<SimpleWorkItem> Parents { get; set; }
        public string AssignedTo { get; set; }
    }
}
