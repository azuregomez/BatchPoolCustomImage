using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace BatchApp
{
   
    public class BatchAgent 
    {
        private BatchConfig _config;        
        private string _poolId;
        private string _jobId;

        public BatchClient Client { get; set; }
        public BatchAgent(BatchConfig batchConfig)
        {
            _config = batchConfig;
            string ticks = System.DateTime.Now.Ticks.ToString();
            _poolId = "Pool" + ticks;
            _jobId = "Job" + ticks;
        }

      

        public void Execute()
        {                        

            string reqfile = _config.ResourceFolder + _config.ResourceName;
            //> CREATE
            try
            {               
                ResourceFile req = CreateResourceFile(reqfile);

                BatchTokenCredentials cred = new BatchTokenCredentials(_config.BatchAccountUrl, GetToken());
                Client = BatchClient.Open(cred);
                ImageReference imgref = new ImageReference(_config.VmImage);
                VirtualMachineConfiguration vmc = new VirtualMachineConfiguration(imageReference: imgref, nodeAgentSkuId: "batch.node.windows amd64");                
                CreateBatchPool(vmc, _config.VmSKU);                
                // Create Job
                CloudJob job = Client.JobOperations.CreateJob();
                job.Id = _jobId;
                job.PoolInformation = new PoolInformation { PoolId = _poolId };
                job.Commit();
                // Create Tasks
                List<CloudTask> tasks = new List<CloudTask>();
                string taskId = "task" + System.DateTime.Now.Ticks.ToString();                 
                string cmd = String.Format("cmd /c type {0}", req.FilePath);
                CloudTask task = new CloudTask(taskId, cmd);
                task.ResourceFiles = new List<ResourceFile> { req };
                tasks.Add(task);
                Client.JobOperations.AddTask(_jobId, tasks);
            }

            catch (Exception x)
            {
                //TODO: Log Somewhere
                throw;
            }            
        }

        private void CreateBatchPool(VirtualMachineConfiguration vmc, string vmsize)
        {
            try
            {
                CloudPool pool = Client.PoolOperations.CreatePool(
                    poolId: _poolId,
                    targetDedicatedComputeNodes: _config.Workers,
                    virtualMachineSize: vmsize,
                    virtualMachineConfiguration: vmc);
                // Specify the application and version to install on the compute nodes
                pool.ApplicationPackageReferences = new List<ApplicationPackageReference>
                {
                    new ApplicationPackageReference {
                        ApplicationId = "NewtonSoftJson",
                        Version = "1.0" }
                };
                pool.Commit();
                
            }
            catch (BatchException be)
            {
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    //Pool exists;
                }
                else
                {
                    throw;
                }
            }
        }

        private ResourceFile CreateResourceFile(string path)
        {            
            CloudStorageAccount sa = CloudStorageAccount.Parse(_config.StorageCnString);
            CloudBlobClient bc = sa.CreateCloudBlobClient();
            string name = Path.GetFileName(path);
            CloudBlobContainer c = bc.GetContainerReference(_config.BlobContainer);
            c.CreateIfNotExists();
            CloudBlockBlob blob = c.GetBlockBlobReference(name);
            blob.UploadFromFile(path);
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(5),
                Permissions = SharedAccessBlobPermissions.Read
            };
            string sastkn = blob.GetSharedAccessSignature(sasConstraints);
            string uri = String.Format("{0}{1}", blob.Uri, sastkn);
            return ResourceFile.FromUrl(uri,name);
        }

        private string GetToken()
        {
            Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext cntx = new AuthenticationContext(_config.AadEndpoint);
            string clientId = _config.BatchApplicationId;
            string clientKey = _config.BatchApplicationKey;        
            System.Threading.Tasks.Task<AuthenticationResult> result = cntx.AcquireTokenAsync(_config.BatchServiceUrl, new ClientCredential(clientId, clientKey));
            result.Wait();
            return result.Result.AccessToken;
        }

    }
}
