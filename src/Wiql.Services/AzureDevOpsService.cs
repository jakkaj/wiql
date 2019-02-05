using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wiql.Contract;
using Wiql.Model.Model.AzureDevOps;

namespace Wiql.Services
{
    

    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly IAzureDevopsAuthService _authService;

        public AzureDevOpsService(IAzureDevopsAuthService authService)
        {
            _authService = authService;
        }

        private const int WI_LIMIT = 400;
        public List<dynamic> _getWorkItems(List<string> origList)
        {
            var wiResult = new List<dynamic>();
            
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
                    return new List<dynamic>();
                }
                var allIdUrl =
                    $"{_authService.GetProjectBaseUrl()}_apis/wit/workitems?ids={ids}&$expand=relations&api-version=5.0";

                var result = _runUrl(allIdUrl);

                dynamic dynResult = JObject.Parse(result);

              //  var testObj = dynResult.value[0].id;

                //var entities = JsonConvert.DeserializeObject<WorkItemGet>(result);
                wiResult.AddRange(dynResult.value);
            }



            return wiResult;
        }

        public async Task<List<dynamic>> RunQuery(string query)
        {
            if (query.IndexOf("{") == -1)
            {
                query = $"{{\"query\":\"{query}\"}}";
            }

            var result = RunQueryRaw(query);

            var queryResult = JsonConvert.DeserializeObject<WorkItemQueryResults>(result);

            var itemIds = queryResult.workItems.Select(_ => _.id.ToString()).ToList();

            var allIds = string.Join(',', itemIds);
            Debug.WriteLine(allIds);

            var workItems = _getWorkItems(itemIds);

            var hierarchy = _getHeirarchyAll(itemIds);

            List<Task<dynamic>> _allTasks = new List<Task<dynamic>>();


            //var together = (workItems, hierarchy);

            //File.WriteAllText(@"C:\Temp\demotemp\demo16\output.json", JsonConvert.SerializeObject(together));


            foreach (var wi in workItems)
            {
                var taskResult = Task.Run(() => _populateItem(wi, hierarchy));
                _allTasks.Add(taskResult);
            }

            var wiResult = await Task.WhenAll(_allTasks);
            var wiList = wiResult.Select(_=>_.Result).ToList();
            wiList.ToList().RemoveAll(_ => _ == null);
            return wiList;
            

        }

        async Task<dynamic> _populateItem(dynamic wi, dynamic relations)
        {
            //var title = wi.fields.Title;

            var url = $"{_authService.GetProjectBaseUrl()}_workitems/edit/{wi.id}";

            //var dtOffset = DateTimeOffset.Parse(startDate, null);

            //var startDateParsed = dtOffset.DateTime;
            //var endDate = _addBusinessDays(startDateParsed, Convert.ToInt32(Math.Ceiling((float.Parse(duration)))));
            

            //Debug.WriteLine($"Title: {title}, Type: {wi.fields.WorkItemType}, Start: {startDateParsed.ToLongDateString()}, Duration: {duration}, End:{endDate.ToLongDateString()}");


            var hierarchy = _getParents(wi, relations);

            var hTitle = "";
            var parentList = new List<dynamic>();

            foreach (var hItem in hierarchy)
            {
                parentList.Add(new
                {
                    Object = hItem,
                    Url = $"{_authService.GetProjectBaseUrl()}_workitems/edit/{hItem.id}"
                });

            }

            var simpleWorkItem = new
            {
                Object = wi,
                Url = url,
                Parents = parentList,
            };
            
            return simpleWorkItem;
        }

        List<dynamic> _getParents(dynamic wi, dynamic relations)
        {

            var id = wi.id;

            var parent = "";

            foreach (var wiCheck in relations.Relations.workItemRelations)
            {
                if (wiCheck.rel == "System.LinkTypes.Hierarchy-Forward" && wiCheck.target.id.ToString() == id.ToString())
                {
                    parent = wiCheck.source.id.ToString();
                    break;
                }
            }

            dynamic parentItem = null;

            foreach (var relItemCheck in relations.WorkItems)
            {
                if (relItemCheck.id == parent)
                {
                    parentItem = relItemCheck;
                    break;
                }
            }
            
            var wiList = new List<dynamic>();

            if (parentItem == null)
            {
                Debug.WriteLine("Skip");
                return wiList;
            }

            wiList.Add(parentItem);

            while (parent != null)
            { 
                foreach (var sourceParent in relations.Relations.workItemRelations)
                {
                    if (sourceParent.rel == "System.LinkTypes.Hierarchy-Forward" &&
                        sourceParent.target.id.ToString() == parent)
                    {
                        parent = sourceParent.source.id.ToString();
                        break;
                    }

                    parent = null;
                }

                if (parent == null)
                {
                    break;
                }

                parentItem = null;

                foreach (var getParentItem in relations.WorkItems)
                {
                    if (getParentItem.id == parent)
                    {
                        parentItem = getParentItem;
                        break;
                    }
                }

                
                if (parentItem == null)
                {
                    continue;
                }
                wiList.Add(parentItem);
            }
            wiList.Reverse();
            return wiList;
        }


        DateTime _addBusinessDays
            (System.DateTime startDate, int daysToAdd)
        {
            var origStart = startDate;
            int index = 0;



            for (index = 1; index <= daysToAdd; index++)
            {
                switch (startDate.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        index -= 1;
                        break;
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        break;
                    case DayOfWeek.Saturday:
                        index -= 1;
                        break;
                }
                startDate = startDate.AddDays(1);
            }

            startDate = startDate.AddDays(1);

            startDate = startDate.AddMilliseconds(-1);

            return startDate;
        }


        dynamic _getHeirarchyAll(List<string> ids)
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

            var resultUp = RunQueryRaw(upQuery);

            var rel = JsonConvert.DeserializeObject<WorkItemRelatedResults>(resultUp);

            var upIds = rel.workItemRelations
                .Where(_ => _.rel == "System.LinkTypes.Hierarchy-Forward")
                .Select(_ => _.source.id.ToString()).ToList();

            upIds = upIds.GroupBy(x => x).Select(y => y.First()).ToList();
            Debug.WriteLine(String.Join(',', upIds));
            var relChainUpWorkItems = _getWorkItems(upIds);

            Debug.WriteLine(string.Join(',', relChainUpWorkItems.Select(_ => _.id)));

            var wiRel = new 
            {
                WorkItems = relChainUpWorkItems,
                Relations = rel
            };

            return wiRel;

            //var wiRel = new WorkItemRelations
            //{
            //    WorkItems = relChainUpWorkItems,
            //    Relations = rel
            //};

            //return wiRel;
        }

        public string RunQueryRaw(string query)
        {
            if (query.IndexOf("{") == -1)
            {
                query = $"{{\"query\":\"{query}\"}}";
            }

            var wc = new WebClient();
            wc.Headers.Add("Authorization", _authService.GetBasicAuth());
            wc.Headers.Add("Content-Type", "application/json");
            var result = wc.UploadString(
                $"{_authService.GetTeamBaseUrl()}_apis/wit/wiql?api-version=1.0",
                query);

            return result;

        }

        string _runUrl(string url)
        {
            var wc = new WebClient();
            wc.Headers.Add("Authorization", _authService.GetBasicAuth());
            wc.Headers.Add("Content-Type", "application/json");
            var result = wc.DownloadString(
                url);

            return result;
        }
    }
}
