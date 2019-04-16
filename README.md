<h3>Azure Batch with Custom Images and Resource File</h3>
This project is a C# Console application that:
<ul>
<li>Creates a Batch Pool using Custom Images
<li>Creates a Job
<li>Submits a Task with a Resource File from Blob Storage
<li>The resource file is copied to the node's working directory.  Which is at D:\batch\tasks\workitems under jobdirectories for jobid, task id and wd. Something like this: \Job636909490764590547\job-1\task636909490827864943\wd
</ul>
Most code is in BatchAgent.cs
<h3>Pre-requisites</h3>
<ol>
<li>Batch Account Created.  <br>
https://docs.microsoft.com/en-us/azure/batch/batch-account-create-portal
<li>Storage Account v2 Created. Hierarchical namespaces should be disabled. This account will be used to upload a Resource File for the Batch Tasks.
<li>Asscociate the Storage Account with the Batch Account in the batch account storage account tab.
<li>Azure Active Directory Web/API App Registration created. This creates a Service Principal to be used by the code. You will need the application Id and Key.
<li>VM Image created from snapshot.  This is used by the code to create a Batch Pool with Custom Images.  To create an image, a VM needs to run sysprep, snapshotted and then image can be created. There is a Powershell image.ps1 included in BatchApp/Powershell that creates snapshot an image after SYSPREP. <br>
https://docs.microsoft.com/en-us/azure/virtual-machines/windows/snapshot-copy-managed-disk<br>
https://docs.microsoft.com/en-us/azure/virtual-machines/windows/capture-image-resource#create-an-image-from-a-snapshot-using-powershell
<li>Contributor Role Assignment on the Batch Account for the created AAD App Regisrtation.
<li>Contributor AAD Role Assignment on the VM Image for the created AAD Registration. This is also included in the Powershell image.ps1.
<li>A resource file created in your local system. This will be uploaded to blob storage by the code and from blob storage deployed to the Batch Nodes.
</ol>
The Submitted Task opens a cmd and just displays the content of the resource file.
<h3>To use this sample code</h3>
<ol>
<li>Load whatever application and code you need to run on the VM that will become a custom image.
<li>Meet the pre-requisites.
<li>Update App.config with your settings.
<li>Modify the cmd line to invoke your code. The resource file will be in the working directory.
</ol>
Batch API Basics:<br>
https://docs.microsoft.com/en-us/azure/batch/batch-api-basics#azure-storage-account<br>
Quickstart with API:<br>
https://docs.microsoft.com/en-us/azure/batch/quick-run-dotnet<br>
Creating and Using Resource Files:<br>
https://docs.microsoft.com/bs-latn-ba/azure/batch/resource-files
Task dependencies:<br>
https://docs.microsoft.com/en-us/azure/batch/batch-task-dependencies
