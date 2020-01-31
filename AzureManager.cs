/*
 * Author: Christian Tsalidis
 * Implementing Azure Storage
 * (Moving from AWS S3 to Azure Storage)
 * I'm using these resources I found as reference:
 * https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-list
 * https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.storage.cloudstorageaccount?view=azure-dotnet
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using UnityEngine;
using Random = System.Random;

public class AzureManager : MonoBehaviour {
    [SerializeField] private string _storageAccountName;

    [SerializeField] private string _accessKey;
    [SerializeField] private string _containerName;
    [SerializeField] private string _fileToDownloadName;

    private void Start() {
        Setup();
    }

    private async Task Setup() {
        await ListBlobs();
    }

    private async Task ListBlobs() {
        CloudBlob blob;
        BlobContinuationToken continuationToken = null;
        try {
            do {
                StorageCredentials storageCredentials = new StorageCredentials(_storageAccountName, _accessKey);
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);

                // Create a blob client for interacting with the blob service.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(_containerName);
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                    useFlatBlobListing: true, BlobListingDetails.Metadata, null, continuationToken, null, null);
                foreach (var blobItem in resultSegment.Results) {
                    // A flat listing operation returns only blobs, not virtual directories.
                    // blob = (CloudBlob) blobItem;
                    blob = (CloudBlob) blobItem;
                    // Write out some blob properties.
                    Debug.Log("Blob name: " + blob.Name);
                    if (blob.Name.Contains("data.zip")) {
                        await DownloadFile(blob, "data.zip");
                    }
                }

                // Get the continuation token and loop until it is null.
                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);
        }
        catch (Exception e) {
            Debug.LogError(e);
        }
    }

    private async Task DownloadFile(CloudBlob blob, string fileName) {
        string tempPath = Path.Combine(Application.persistentDataPath, "temp", fileName);
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "temp"))) {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "temp"));
        }
        await blob.DownloadToFileAsync(tempPath, FileMode.OpenOrCreate);
    }
}