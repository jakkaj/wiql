﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wiql.Contract;
using Wiql.Model.Model;
using Wiql.Tests.Base;

namespace Wiql.Tests.Tests
{
    [TestClass]
    public class BasicQueryTests : TestBase
    {
        [TestMethod]
        public void TestGetSettings()
        {
            var settings = Resolve<IOptions<AzureDevOpsSettings>>();
            Assert.IsNotNull(settings.Value.PersonalAccessToken);
        }

        [TestMethod]
        public void TestGetBasicAuth()
        {
            var settings = Resolve<IOptions<AzureDevOpsSettings>>();
            var authService = Resolve<IAzureDevopsAuthService>();

            var encoded = authService.GetBasicAuth(settings.Value.PersonalAccessToken, settings.Value.UserEmail);

            Assert.IsNotNull(encoded);

        }
    }
}
