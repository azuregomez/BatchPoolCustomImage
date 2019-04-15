using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace BatchApp1
{
    public static class BatchAgent
    {
        public static string AuthorityUri { get; set; }
        public static string ClientId { get; set; }
        public static string RedirectUri { get; set; }
        public static string BatchResourceUri { get; set; }

        public static string BatchAccountUrl { get; set; }

        
        public static async Task<string> GetAuthenticationTokenAsync()
        {
            var authContext = new AuthenticationContext(AuthorityUri);
            // Acquire the authentication token from Azure AD.
            var authResult = await authContext.AcquireTokenAsync(BatchResourceUri,
                                                                ClientId,
                                                                new Uri(RedirectUri),
                                                                new PlatformParameters(PromptBehavior.Auto));
            return authResult.AccessToken;
        }

        public static async Task PerformBatchOperations()
        {
            Func<Task<string>> tokenProvider = () => GetAuthenticationTokenAsync();

            using (var client = BatchClient.Open(new BatchTokenCredentials(BatchAccountUrl, tokenProvider)))
            {
                await client.JobOperations.ListJobs().ToListAsync();
            }
        }
    }
}
