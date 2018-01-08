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
            AwsClient.Initialize(RegionEndpoint.EUWest1);

            AwsS3SynapseFile file = new AwsS3SynapseFile(@"s3://wagug0-test/Destination/GuyWasHere.txt");
            file.Create(null, false, "Test", ConsoleWriter);
            file.CloseStream();

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        private static void ConsoleWriter(string label, string message)
        {
            Console.WriteLine( $"{label} - {message}" );
        }
    }
}
