using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Synapse.Filesystem
{
    public static class AwsClient
    {
        public static AmazonS3Client Client { get; internal set; }

        /// <summary>
        /// Initialize S3Client using implicit Credentials from config or profile.
        /// </summary>
        /// <param name="endpoint">The region to connect to.</param>
        public static void Initialize(RegionEndpoint endpoint = null)
        {
            if (Client != null)
                Client = null;

            if (endpoint == null)
                Client = new AmazonS3Client();
            else
                Client = new AmazonS3Client(endpoint);
        }

        /// <summary>
        /// Initialize S3Client using AWSCredentials object.
        /// </summary>
        /// <param name="creds">The AWSCredentails object.</param>
        /// <param name="endpoint">The region to connect to.</param>
        public static void Initialize(AWSCredentials creds, RegionEndpoint endpoint = null)
        {
            if (Client != null)
                Client = null;

            if (endpoint == null)
                Client = new AmazonS3Client(creds);
            else
                Client = new AmazonS3Client(creds, endpoint);
        }

        /// <summary>
        /// Initialize S3Client using a BasicAWSCredentials object.
        /// </summary>
        /// <param name="accessKey">AWS Access Key Id</param>
        /// <param name="secretAccessKey">AWS Secret Access Key</param>
        /// <param name="endpoint">The region to connect to.</param>
        public static void Initialize(string accessKey, string secretAccessKey, RegionEndpoint endpoint = null)
        {
            BasicAWSCredentials creds = new BasicAWSCredentials(accessKey, secretAccessKey);
            Initialize(creds, endpoint);
        }

        public static void Initialize(string accessKey, string secretAccessKey, string sessionToken, RegionEndpoint endpoint = null)
        {
            SessionAWSCredentials sessionCreds = new SessionAWSCredentials(accessKey, secretAccessKey, sessionToken);
            Initialize(sessionCreds, endpoint);
        }

        public static void Initialize(string profileName, RegionEndpoint endpoint = null)
        {
            CredentialProfileStoreChain chain = new CredentialProfileStoreChain();
            AWSCredentials creds = null;
            if (chain.TryGetAWSCredentials(profileName, out creds))
                Initialize(creds, endpoint);
            else
                throw new Exception($"Unable To Retrieve Credentails For Profile [{profileName}]");
        }

        public static void Close()
        {
            Client = null;
        }
    }
}
