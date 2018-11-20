using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SSMVCCoreApp.Infrastructure.Abstract;
using SSMVCCoreApp.Infrastructure.Concrete;

namespace SSMVCCoreApp.Infrastructure.Services
{
  public class PhotoService : IPhotoService
  {
    private readonly ILogger<PhotoService> _logger;
    CloudStorageAccount _storageAccount;

    public PhotoService(IOptions<StorageUtility> storageUtility, ILogger<PhotoService> logger)
    {
      _logger = logger;
      _storageAccount = storageUtility.Value.StorageAccount;
    }


        #region IPhotoService Member
        public async Task<string> UploadPhotoAsync(string category, IFormFile photoToUpload)
        {
            if (photoToUpload == null || photoToUpload.Length == 0)
            {
                return null;
            }
            string fullPath = null;
            Stopwatch timespan = Stopwatch.StartNew();
            try
            {
                //Create a blob client and retrive reference for the category container
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

                CloudBlobContainer blobContainer = blobClient.GetContainerReference(category.ToLower().Trim());

                if (await blobContainer.CreateIfNotExistsAsync())
                {
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                    _logger.LogInformation($"Successfully created blob storage '{blobContainer.Name}' Container and made it public");
                }
                //create a unique name for the image to be uploaded
                //string imageName = $"productphoto{DateTime.UtcNow.ToString()}{Path.GetExtension(photoToUpload.FileName.Substring(photoToUpload.FileName.LastIndexOf("/") + 1))}";
                string imageName = $"productphoto{Guid.NewGuid().ToString()}{Path.GetExtension(photoToUpload.FileName.Substring(photoToUpload.FileName.LastIndexOf("/") + 1))}";

                //upload image to blob storage
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(imageName);
                blockBlob.Properties.ContentType = photoToUpload.ContentType;
                await blockBlob.UploadFromStreamAsync(photoToUpload.OpenReadStream());

                fullPath = blockBlob.Uri.ToString();
                timespan.Stop();
                _logger.LogInformation($"blob service, PhotoService.UpLoadPhoto, TimeElapsed = {timespan.Elapsed}, imagepath={fullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errror Uploading the photoblob to storage");
                throw;
            }
            return fullPath;

        }

            public async Task<bool> DeletePhotoAsync(string category, string photoUrl)
    {
                if (string.IsNullOrEmpty(photoUrl))
                {
                    return true;
                }
                Stopwatch timespan = Stopwatch.StartNew();
                bool deleteFlag = false;
                try
                {
                    CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(category.ToLower().Trim());

                    if (blobContainer.Name == category.ToLower().Trim())
                    {
                        string blobName = photoUrl.Substring(photoUrl.LastIndexOf("/") + 1);
                        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
                        deleteFlag = await blockBlob.DeleteIfExistsAsync();

                    }
                    timespan.Stop();
                    _logger.LogInformation($"Blob Service, PhotoService.DeletePhoto, TimeElapsed - {timespan.Elapsed}, deletedimagepath={photoUrl}");
                    return deleteFlag;
                    //Delete the containers if it is empty
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Deleting the photo blob from storage");
                    throw;
                }
            }
    #endregion
  }
}