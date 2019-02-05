using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Wiql.Model.Model.AzureDevOps
{
    public class WorkItemData
    {
        public int id { get; set; }
        public int rev { get; set; }
        public WorkItemFields fields { get; set; }

        public string url { get; set; }
        public List<Relation> relations { get; set; }
        public Links _links { get; set; }
        public int parent { get; set; }
        public List<int> children { get; set; }
    }

    public class Relation
    {
        public string rel { get; set; }
        public string url { get; set; }

    }

    public class Html
    {
        public string href { get; set; }
    }

    public class Links
    {

        public Html html { get; set; }

    }

    public class WorkItemFields
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string WorkItemType { get; set; }

        [JsonProperty(PropertyName = "CSEngineering.ParticipationDurationDays")]
        public string ParticipationDurationDays { get; set; }

        [JsonProperty(PropertyName = "System.AssignedTo")]
        public string AssignedTo { get; set; }

        [JsonProperty(PropertyName = "CSEngineering.ParticipationStartDate")]
        public string ParticipationStartDate { get; set; }

        [JsonProperty(PropertyName = "CSEngineering.ActivityStartDate")]
        public string ActivityStartDate { get; set; }

        [JsonProperty(PropertyName = "CSEngineering.ActivityDuration")]
        public string ActivityDuration { get; set; }

        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }
    }
}
