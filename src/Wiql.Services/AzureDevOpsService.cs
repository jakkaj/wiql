using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        public List<WorkItemData> _getWorkItems(List<string> origList)
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
                    $"{_authService.GetProjectBaseUrl()}_apis/wit/workitems?ids={ids}&$expand=relations&api-version=5.0";

                var result = _runUrl(allIdUrl);

                var entities = JsonConvert.DeserializeObject<WorkItemGet>(result);
                wiResult.AddRange(entities.value);
            }



            return wiResult;
        }

        public async Task<List<SimpleWorkItem>> RunQuery(string query)
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

            List<Task<SimpleWorkItem>> _allTasks = new List<Task<SimpleWorkItem>>();


            //var together = (workItems, hierarchy);

            //File.WriteAllText(@"C:\Temp\demotemp\demo16\output.json", JsonConvert.SerializeObject(together));


            foreach (var wi in workItems)
            {
                var taskResult = Task.Run(() => _populateItem(wi, hierarchy));
                _allTasks.Add(taskResult);
            }

            var wiResult = await Task.WhenAll(_allTasks);
            var wiList = wiResult.ToList();
            wiList.ToList().RemoveAll(_ => _ == null);
            return wiList;
            

        }

        async Task<SimpleWorkItem> _populateItem(WorkItemData wi, WorkItemRelations relations)
        {
            var title = wi.fields.Title;

            var startDate = wi.fields.ParticipationStartDate ?? wi.fields.ActivityStartDate;
            var duration = wi.fields.ParticipationDurationDays ??
                           wi.fields.ActivityDuration;

            if (startDate == null)
            {
                return null;
            }

            var url = $"{_authService.GetBasicAuth()}_workitems/edit/{wi.id}";

            var dtOffset = DateTimeOffset.Parse(startDate, null);

            var startDateParsed = dtOffset.DateTime;
            var endDate = _addBusinessDays(startDateParsed, Convert.ToInt32(Math.Ceiling((float.Parse(duration)))));
            

            Debug.WriteLine($"Title: {title}, Type: {wi.fields.WorkItemType}, Start: {startDateParsed.ToLongDateString()}, Duration: {duration}, End:{endDate.ToLongDateString()}");


            var hierarchy = _getParents(wi, relations);

            var simpleWorkItem = new SimpleWorkItem
            {
                EndDate = endDate,
                StartDate = startDateParsed,
                Title = title,
                Url = url,
                Parents = new List<SimpleWorkItem>(),
                AssignedTo = wi.fields.AssignedTo
            };

            var hTitle = "";

            foreach (var hItem in hierarchy)
            {
                simpleWorkItem.Parents.Add(new SimpleWorkItem
                {
                    Title = hItem.fields.Title,
                    Url = $"{_authService.GetProjectBaseUrl()}_workitems/edit/{hItem.id}"
                });
                hTitle += hItem.fields.Title + "\\";
            }
            simpleWorkItem.FullTitle = hTitle + simpleWorkItem.Title;

            if (wi.fields.WorkItemType == "Activity")
            {
                if (simpleWorkItem.Parents.Count > 0)
                {
                    simpleWorkItem.Title = simpleWorkItem.Parents[simpleWorkItem.Parents.Count - 2].Title + "\\" + simpleWorkItem.Parents.Last().Title + "\\" + simpleWorkItem.Title;
                }
            }
            else
            {
                if (simpleWorkItem.Parents.Count > 2)
                {
                    simpleWorkItem.Title = simpleWorkItem.Parents[simpleWorkItem.Parents.Count - 3].Title + "\\" + simpleWorkItem.Parents[simpleWorkItem.Parents.Count - 2].Title + "\\" + simpleWorkItem.Parents.Last().Title;
                }
            }

            return simpleWorkItem;
        }

        List<WorkItemData> _getParents(WorkItemData wi, WorkItemRelations relations)
        {

            var id = wi.id;
            var parent = relations.Relations.workItemRelations
                .Where(_ => _.rel == "System.LinkTypes.Hierarchy-Forward" && _.target.id == id)
                .Select(_ => _.source.id.ToString()).FirstOrDefault();


            var parentItem = relations.WorkItems.FirstOrDefault(_ => _.id.ToString() == parent);

            var wiList = new List<WorkItemData>();

            if (parentItem == null)
            {
                Debug.WriteLine("Skip");
                return wiList;
            }

            wiList.Add(parentItem);

            while (parent != null)
            {
                parent = relations.Relations.workItemRelations
                    .Where(_ => _.rel == "System.LinkTypes.Hierarchy-Forward" && _.target.id.ToString() == parent)
                    .Select(_ => _.source.id.ToString()).FirstOrDefault();
                parentItem = relations.WorkItems.FirstOrDefault(_ => _.id.ToString() == parent);

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


        WorkItemRelations _getHeirarchyAll(List<string> ids)
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

            var wiRel = new WorkItemRelations
            {
                WorkItems = relChainUpWorkItems,
                Relations = rel
            };

            return wiRel;
        }

        public string RunQueryRaw(string query)
        {
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
