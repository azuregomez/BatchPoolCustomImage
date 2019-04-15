using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BatchApp
{
    public class BatchConfig
    {
        public const string WORKERS = "workers";
        public const string BATCH_ACCOUNT_URL = "batch_account_url";
        public const string BATCH_ACCOUNT_NAME = "batch_account_name";        
        public const string VM_IMAGE = "vm_image";
        public const string VM_SKU = "vm_sku";
        public const string BATCH_APPLICATION_ID = "batch_application_id";
        public const string BATCH_APPLICATION_KEY = "batch_applcation_key";                
        public const string RESOURCE_FOLDER = "resourcefile_folder";
        public const string RESOURCE_NAME = "resourcefile_name";
        public const string STORAGE_CNSTR = "storage_cnstr";
        public const string BLOB_CONTAINER = "blob_container";
        public const string AAD_ENDPOINT = "aad_endpoint";

        public int Workers { get; set; }
        public string BatchAccountUrl { get; set; }
        public string BatchAccountName { get; set; }
        public string VmImage { get; set; }
        public string BatchApplicationId { get; set; }
        public string BatchApplicationKey { get; set; }
        public string BatchServiceUrl {  get { return "https://batch.core.windows.net/"; } }
        public string ResourceFolder { get; set; }
        public string ResourceName { get; set; }
        public string StorageCnString { get; set; }
        public string BlobContainer { get; set; }
        public string VmSKU { get; set; }
        public string AadEndpoint { get; set; }

        public static BatchConfig FromAppSettings()
        {
            return new BatchConfig()
            {
                Workers = Int32.Parse(ConfigurationManager.AppSettings[WORKERS]),
                BatchAccountUrl = ConfigurationManager.AppSettings[BATCH_ACCOUNT_URL],
                BatchAccountName = ConfigurationManager.AppSettings[BATCH_ACCOUNT_NAME],                
                VmImage = ConfigurationManager.AppSettings[VM_IMAGE],
                BatchApplicationId = ConfigurationManager.AppSettings[BATCH_APPLICATION_ID],
                BatchApplicationKey = ConfigurationManager.AppSettings[BATCH_APPLICATION_KEY],
                ResourceFolder = ConfigurationManager.AppSettings[RESOURCE_FOLDER],
                ResourceName = ConfigurationManager.AppSettings[RESOURCE_NAME],
                StorageCnString = ConfigurationManager.AppSettings[STORAGE_CNSTR],
                BlobContainer = ConfigurationManager.AppSettings[BLOB_CONTAINER],
                AadEndpoint = ConfigurationManager.AppSettings[AAD_ENDPOINT],
                VmSKU = ConfigurationManager.AppSettings[VM_SKU]
            };
        }
    }
}
