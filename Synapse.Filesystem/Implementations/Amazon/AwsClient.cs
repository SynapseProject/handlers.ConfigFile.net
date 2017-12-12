using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;

namespace Synapse.Filesystem
{
    public static class AwsClient
    {
        public static AmazonS3Client Client { get; internal set; }

        public static void Initialize(RegionEndpoint endpoint)
        {
                if ( Client != null )
                    Client = null;

            Client = new AmazonS3Client( endpoint );
        }

        public static void Close()
        {
            Client = null;
        }
    }
}
