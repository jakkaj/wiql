using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Wiql.Contract;
using Wiql.Tests.Base;

namespace Wiql.Tests.Tests
{
    [TestClass]
    public class ComplexQueryTests : TestBase
    {
        [TestMethod]
        public async Task TestComplexQueryGetIds()
        {
            var service = Resolve<IAzureDevOpsService>();

            var query = File.ReadAllText("TestData/ComplexQuery.txt");

            Assert.IsNotNull(query);

            var queryResult = await service.RunQuery(query);

            Assert.IsNotNull(queryResult);

            var json = JsonConvert.SerializeObject(queryResult);

            Debug.WriteLine(json);

            Assert.IsTrue(queryResult.Count > 0);
        }
    }
}
