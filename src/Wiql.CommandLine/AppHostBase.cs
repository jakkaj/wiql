using System;
using System.Collections.Generic;
using System.Text;
using Wiql.Services.ServiceSetup;

namespace Wiql.CommandLine
{
    public class AppHostBase : AppHost<AppHostBase>
    {
        public AppHostBase()
        {
            Boot();
        }
    }
}
