using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Amazon;
using Alphaleonis.Win32.Filesystem;

namespace Synapse.Filesystem
{
    public class Sandbox
    {
        public static void Main(string[] args)
        {
            AwsClient awsClient = new AwsClient(RegionEndpoint.USEast1);
            AwsS3SynapseDirectory dir = new AwsS3SynapseDirectory(awsClient, @"s3://wagug0-test/");

            foreach (SynapseDirectory d in dir.GetDirectories())
                Console.WriteLine(d.FullName);

            foreach (SynapseFile f in dir.GetFiles())
                Console.WriteLine(f.FullName);

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        private static void ConsoleWriter(string label, string message)
        {
            Console.WriteLine( $"{label} - {message}" );
        }
    }
}
