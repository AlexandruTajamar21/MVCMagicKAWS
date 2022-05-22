using Amazon.S3;
using Amazon.S3.Model;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MVCMagicK.Services
{
    public class ServiceStorageBlob
    {
        private string bucketName;
        private IAmazonS3 awsClient;

        public ServiceStorageBlob(IAmazonS3 client, IConfiguration configuration)
        {

            this.awsClient = client;
            this.bucketName = configuration.GetValue<string>("AWS:bucketcartas");
        }
        public async Task<bool> UploadFileAsync(Stream stream, string fileName)
        {

            PutObjectRequest request = new PutObjectRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = this.bucketName
            };
            PutObjectResponse response = await this.awsClient.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            DeleteObjectResponse response = await this.awsClient.DeleteObjectAsync(this.bucketName, fileName);


            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<Stream> GetFileAsync(string fileName)
        {
            GetObjectResponse response = await this.awsClient.GetObjectAsync(this.bucketName, fileName);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.ResponseStream;
            }
            else
            {
                return null;
            }
        }
    }
}
