using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.Runtime;

namespace Synapse.Filesystem
{
    public static class AwsClient
    {
        public static AmazonS3Client Client { get; internal set; }

        public static void Initialize()
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client();
        }

        public static void Initialize(RegionEndpoint endpoint)
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client(endpoint);
        }

        public static void Initialize(AWSCredentials creds)
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client(creds);
        }

        public static void Initialize(AWSCredentials creds, RegionEndpoint endpoint)
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client(creds, endpoint);
        }

        public static void Initialize(string accessKey, string secretAccessKey)
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client(accessKey, secretAccessKey);
        }

        public static void Initialize(string accessKey, string secretAccessKey, RegionEndpoint endpoint)
        {
            if (Client != null)
                Client = null;
            Client = new AmazonS3Client(accessKey, secretAccessKey, endpoint);
        }


        public static void Close()
        {
            Client = null;
        }
    }
}
