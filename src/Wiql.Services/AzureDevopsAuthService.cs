using System;
using System.Collections.Generic;
using System.Text;
using Wiql.Contract;

namespace Wiql.Services
{
    public class AzureDevopsAuthService : IAzureDevopsAuthService
    {
        public string GetBasicAuth(string pat, string userEmail)
        {
            var bytes = Encoding.UTF8.GetBytes($"{userEmail}:{pat}");

            var b64 = Convert.ToBase64String(bytes);

            var result = $"Basic {b64}";

            return result;
        }
    }
}
