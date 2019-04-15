using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;

namespace BatchApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string tenantId = ConfigurationManager.AppSettings["activeDirectoryTenantId"];

            string batchResourceUri = "https://batch.core.windows.net/";
            string authorityUri = String.Format("https://login.microsoftonline.com/{0}",tenantId);
            string batchAccountUrl = ConfigurationManager.AppSettings["bathAccountUrl"];
            string clientId = ConfigurationManager.AppSettings["applicationId"];
            string redirectUri = ConfigurationManager.AppSettings["redirectUri"];


            

        }


        
    }
}
