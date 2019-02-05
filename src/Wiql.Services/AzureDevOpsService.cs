using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Wiql.Services
{
    public class AzureDevOpsService
    {
        public List<WorkItemData> _getWorkItems(string token, List<string> origList)
        {
            var wiResult = new List<WorkItemData>();

            var idList = origList.ToList();

            while (idList.Count > 0)
            {
                var lTemp = new List<string>();
                while (idList.Count > 0 && lTemp.Count < WI_LIMIT)
                {
                    lTemp.Add(idList[0]);
                    idList.RemoveAt(0);
                }

                if (lTemp.Count == 0)
                {
                    break;
                }
                var ids = string.Join(',', lTemp);
                Debug.WriteLine($"H: {ids}");


                if (string.IsNullOrWhiteSpace(ids))
                {
                    return new List<WorkItemData>();
                }
                var allIdUrl =
                    $"https://<>.visualstudio.com/DefaultCollection/<>/_apis/wit/workitems?ids={ids}&$expand=relations&api-version=4.1-preview";

                var result = _runUrl(token, allIdUrl);

                var entities = JsonConvert.DeserializeObject<WorkItemGet>(result);
                wiResult.AddRange(entities.value);
            }



            return wiResult;
        }

        WorkItemRelations _getHeirarchyAll(string token, List<string> ids)
        {

            var idGroup = "";

            foreach (var id in ids)
            {

                if (idGroup != "")
                {
                    idGroup += " OR ";
                }
                idGroup += $" Target.[System.Id] = '{id}' ";
            }

            if (ids.Count > 1)
            {
                idGroup = $"({idGroup})";
            }

            var upQuery =
                $"{{\"query\":\"select [System.Id], [System.WorkItemType], [System.Title], [System.AssignedTo], [System.State] from WorkItemLinks where ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') and {idGroup} order by [System.Id] mode (Recursive, ReturnMatchingChildren)\"}}";

            var resultUp = RunQuery(token, upQuery);
            var rel = JsonConvert.DeserializeObject<WorkItemRelatedResults>(resultUp);

            var upIds = rel.workItemRelations
                .Where(_ => _.rel == "System.LinkTypes.Hierarchy-Forward")
                .Select(_ => _.source.id.ToString()).ToList();

            upIds = upIds.GroupBy(x => x).Select(y => y.First()).ToList();
            Debug.WriteLine(String.Join(',', upIds));
            var relChainUpWorkItems = _getWorkItems(token, upIds);

            Debug.WriteLine(string.Join(',', relChainUpWorkItems.Select(_ => _.id)));

            var wiRel = new WorkItemRelations
            {
                WorkItems = relChainUpWorkItems,
                Relations = rel
            };

            return wiRel;
        }

        public string RunQuery(string query)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", "Bearer " + token);
            wc.Headers.Add("Content-Type", "application/json");
            var result = wc.UploadString(
                "https://<>.visualstudio.com/DefaultCollection/<>/_apis/wit/wiql?api-version=1.0",
                query);

            return result;

        }

        string _runUrl(string token, string url)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", "Bearer " + token);
            wc.Headers.Add("Content-Type", "application/json");
            var result = wc.DownloadString(
                url);

            return result;
        }
    }
}
