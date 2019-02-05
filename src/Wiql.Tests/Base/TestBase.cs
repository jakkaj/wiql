using Wiql.Services.ServiceSetup;

namespace Wiql.Tests.Base
{
    public class TestBase : AppHost<TestBase>
    {
        public TestBase()
        {
            Boot();
        }
    }
}
