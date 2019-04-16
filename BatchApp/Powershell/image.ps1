# First: sysprep: https://docs.microsoft.com/en-us/azure/virtual-machines/windows/capture-image-resource
# Sysprep directory C:\Windows\System32\Sysprep
$vmName = "batchvm"
$rgName = "batch-rg"
$location = "CentralUS"
$snapshotName = "batchvmsnapshot"
$imageName = "batchimage"
Stop-AzVM -ResourceGroupName $rgName -Name $vmName -Force
Set-AzVm -ResourceGroupName $rgName -Name $vmName -Generalized
$vm = get-azvm -ResourceGroupName $resourceGroupName -Name $vmName
$snapshotcfg =  New-AzSnapshotConfig -SourceUri $vm.StorageProfile.OsDisk.ManagedDisk.Id -Location $location -CreateOption copy
New-AzSnapshot -Snapshot $snapshotcfg -SnapshotName $snapshotName -ResourceGroupName $resourceGroupName
$snapshot = Get-AzSnapshot -ResourceGroupName $rgName -SnapshotName $snapshotName
$imageConfig = New-AzImageConfig -Location $location
$imageConfig = Set-AzImageOsDisk -Image $imageConfig -OsState Generalized -OsType Windows -SnapshotId $snapshot.Id
New-AzImage -ImageName $imageName -ResourceGroupName $rgName -Image $imageConfig
