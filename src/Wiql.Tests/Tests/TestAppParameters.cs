using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wiql.Tests.Tests
{
    [TestClass]
    public class TestAppParameters
    {
        [TestMethod]
        public async Task TestBasicParams()
        {
            await CommandLine.Program.Main("some pat");

            Assert.AreEqual(Environment.ExitCode, 0);
            Assert.AreEqual(Environment.GetEnvironmentVariable("AzureDevOpsSettings__PersonalAccessToken"), "some pat");

        }
    }
}
