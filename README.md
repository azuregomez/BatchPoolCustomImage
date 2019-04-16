<h3>Azure Batch with Custom Images</h3>
This project is a C# Console application that:
<ul>
<li>Creates a Batch Pool using Custom Images
<li>Creates a Job
<li>Submits a Task with a Resource File from Blob Storage
</ul>
Most code is in BatchAgent.cs
<h3>Pre-requisites</h3>
<ol>
<li>Batch Account Created
<li>Storage Account v2 Created. Hierarchical namespaces should be disabled. This account will be used to upload a Resource File for the Batch Tasks.
<li>Azure Active Directory Web/API App Registration created. This creates a Service Principal to be used by the code. You will need the application Id and Key.
<li>VM Image created from snapshot.  This is used by the code to create a Batch Pool with Custom Images.  To create an image, a VM needs to run sysprep, snapshotted and then image can be created. There is a Powershell image.ps1 included in BatchApp/Powershell that creates snapshot an image after SYSPREP.
<li>Contributor Role Assignment on the Batch Account for the created AAD App Regisrtation.
<li>Contributor AAD Role Assignment on the VM Image for the created AAD Registration. This is also included in the Powershell image.ps1.
<li>A resource file created in your local system. This will be uploaded to blob storage by the code and from blob storage deployed to the Batch Nodes.
</ol>
