using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchApp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using gCEL.Core.Interfaces;
    using gCEL.Core.Kernel;
    using gCEL.Core.Kernel.Messages;
    using gCEL.Azure.Fabric;
    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Auth;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    namespace gCEL.Azure.Fabric.AB
    {
        public class AzureBatchComputeFabric : AzureComputeFabric
        {
            public const string SETTING_WORKERS = "workers";

            public const string SETTING_BATCH_ACCOUNT_URL = "batch_account_url";
            public const string SETTING_BATCH_ACCOUNT_NAME = "batch_account_name";
            public const string SETTING_BATCH_ACCOUNT_KEY = "batch_account_key";

            public const string SETTING_BATCH_CUSTOM_IMAGE = "batch_custom_image";

            public const string SETTING_BATCH_APPLICATION_ID = "batch_application_id";
            public const string SETTING_BATCH_APPLICATION_KEY = "batch_applcation_key";
            public const string SETTING_BATCH_RESOURCE_URL = "batch_resource_url";
            public const string SETTING_BATCH_AUTHORITY_URI = "batch_authority_uri";

            public const string SETTING_SYSTEM_FOLDER = "system_folder";

            public const string SETTING_STORAGE_CONNECTION = "storage_connection";
            public const string SETTING_STORAGE_CONTAINER = "storage_container";

            protected override void Init()
            {
                base.Init();

                PoolId = "gCELPool" + gCEL.Core.Utils.Helpers.CurrentTicks();
                JobId = "gCELJob" + gCEL.Core.Utils.Helpers.CurrentTicks();

                PoolSize = 0;
            }

            protected override void OnExpand()
            {
                int requirement = int.Parse(RuntimeSettings[gCEL.Core.Components.FABRIC.Constants.CAPACITY_SETTING_REQUIREMENT]);
                int workers = int.Parse(Settings[SETTING_WORKERS]);

                PoolSize = (requirement / workers) + ((requirement % workers) > 0 ? 1 : 0);
                if (PoolSize == 0) PoolSize = 1;

                string reqfile = gCEL.Core.Utils.FileSystem.GetTempPath() + "gcel_request_" + gCEL.Core.Utils.Environment.UniqueIdentifier + ".xml";

                //> CREATE
                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    ResourceFile req = CreateResourceFile(reqfile);

                    //BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(Settings[SETTING_BATCH_ACCOUNT_URL], Settings[SETTING_BATCH_ACCOUNT_NAME], Settings[SETTING_BATCH_ACCOUNT_KEY]);
                    BatchTokenCredentials cred = new BatchTokenCredentials(Settings[SETTING_BATCH_ACCOUNT_URL], GetToken());

                    Client = BatchClient.Open(cred);

                    //> VM
                    ImageReference imgref = new ImageReference(Settings[SETTING_BATCH_CUSTOM_IMAGE]);
                    VirtualMachineConfiguration vmc = new VirtualMachineConfiguration(imageReference: imgref, nodeAgentSkuId: "batch.node.windows amd64");

                    //> POOL
                    CreateBatchPool(vmc);

                    //> JOB
                    CloudJob job = Client.JobOperations.CreateJob();
                    job.Id = JobId;
                    job.PoolInformation = new PoolInformation { PoolId = PoolId };

                    job.Commit();

                    //> TASKS
                    List<CloudTask> tasks = new List<CloudTask>();

                    for (int i = 0; i < requirement; i++)
                    {
                        string taskId = "gCELTask" + i;
                        string engine = Settings[SETTING_SYSTEM_FOLDER] + gCEL.Core.Constants.ENGINE_CMD;
                        string cmd = String.Format("cmd /c {0} command=host component=engine request={1}", engine, req.FilePath);

                        CloudTask task = new CloudTask(taskId, cmd);
                        task.ResourceFiles = new List<ResourceFile> { req };
                        tasks.Add(task);
                    }

                    Client.JobOperations.AddTask(JobId, tasks);

                    //>
                    sw.Stop();

                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Logger.Info(Tag + ":POOLCREATED:" + elapsedTime);
                }

                catch (Exception x)
                {
                    Logger.Error(Tag + ":" + x.Message);
                    throw;
                }

                finally
                {
                    gCEL.Core.Utils.FileSystem.SafeDelete(reqfile);
                    OnContract();
                }

                Logger.Info(Tag + ":ALLOCATED:" + PoolSize);
            }

            protected override void OnContract()
            {
                if (Client != null)
                {
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromMinutes(30);

                        IEnumerable<CloudTask> tasks = Client.JobOperations.ListTasks(JobId);

                        Client.Utilities.CreateTaskStateMonitor().WaitAll(tasks, TaskState.Completed, timeout);

                        Client.JobOperations.DeleteJob(JobId);
                        Client.PoolOperations.DeletePool(PoolId);
                    }
                    catch (Exception x)
                    {
                        Logger.Error(Tag + ":" + x.Message);
                    }
                }
            }

            public override int Size
            {
                get
                {
                    return PoolSize;
                }
            }

            private int PoolSize { get; set; }

            public string PoolId { get; set; }
            public string JobId { get; set; }

            public BatchClient Client { get; set; }

            private void CreateBatchPool(VirtualMachineConfiguration vmc, string vmsize = "Standard_E2_v3")
            {
                try
                {
                    CloudPool pool = Client.PoolOperations.CreatePool(PoolId, vmsize, vmc, PoolSize);
                    pool.Commit();
                }
                catch (BatchException be)
                {
                    if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                    {
                        Logger.Warning("The pool " + PoolId + " already exist");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            private ResourceFile CreateResourceFile(string path)
            {
                gCEL.Core.Utils.FileSystem.WriteStringToFile(path, Request.Xml);

                //>
                CloudStorageAccount sa = CloudStorageAccount.Parse(Settings[SETTING_STORAGE_CONNECTION]);

                CloudBlobClient bc = sa.CreateCloudBlobClient();

                string name = Path.GetFileName(path);

                CloudBlobContainer c = bc.GetContainerReference(Settings[SETTING_STORAGE_CONTAINER]);
                c.CreateIfNotExists();

                //>
                CloudBlockBlob blob = c.GetBlockBlobReference(name);
                blob.UploadFromFile(path);

                SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(5),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                string sastkn = blob.GetSharedAccessSignature(sasConstraints);
                string uri = String.Format("{0}{1}", blob.Uri, sastkn);

                return ResourceFile.FromStorageContainerUrl(uri);
            }

            private string GetToken()
            {
                AuthenticationContext cntx = new AuthenticationContext(Settings[SETTING_BATCH_AUTHORITY_URI]);

                string clientId = Settings[SETTING_BATCH_APPLICATION_ID];
                string clientKey = Settings[SETTING_BATCH_APPLICATION_KEY];

                System.Threading.Tasks.Task<AuthenticationResult> r = cntx.AcquireTokenAsync(Settings[SETTING_BATCH_RESOURCE_URL], new ClientCredential(clientId, clientKey));
                r.Wait();

                return r.Result.AccessToken;
            }

        }
    }

}
