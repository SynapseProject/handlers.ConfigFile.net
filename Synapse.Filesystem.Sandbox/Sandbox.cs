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

            // Copy File From Local To S3
            //String sourceFile = @"C:\Temp\Source\YamlDotNet.xml";
            //String targetFile = @"s3://wagug0-test/Destination/YamlDotNet.xml";
            //WindowsSynapseFile source = new WindowsSynapseFile(sourceFile);
            //AwsS3SynapseFile target = new AwsS3SynapseFile(targetFile);
            //source.CopyTo(target, true, "Sandbox", ConsoleWriter);

            //// Copy Directory From Local To S3
            //String sourceDir = @"C:\Temp\Source\";
            //String targetDir = @"s3://wagug0-test/Destination/";
            //WindowsSynapseDirectory source = new WindowsSynapseDirectory(sourceDir);
            //AwsS3SynapseDirectory target = new AwsS3SynapseDirectory(targetDir);
            //source.CopyTo(target, true, true, "Sandbox", ConsoleWriter);

            // Copy Directory From S3 To Local
            //String sourceDir = @"s3://wagug0-test/Destination/";
            //String targetDir = @"C:\Temp\Destination\";
            //AwsS3SynapseDirectory source = new AwsS3SynapseDirectory(sourceDir);
            //WindowsSynapseDirectory target = new WindowsSynapseDirectory(targetDir);
            //source.CopyTo(target, true, true, "Sandbox", ConsoleWriter);

            // Copy File (Local) To Directory (Local)
            //String sourceFile = @"C:\Temp\Source\YamlDotNet.xml";
            //String targetDir = @"C:\Temp\Destination\";
            //WindowsSynapseFile source = new WindowsSynapseFile(sourceFile);
            //WindowsSynapseDirectory target = new WindowsSynapseDirectory(targetDir);
            //source.CopyTo(target, true, "Sandbox", ConsoleWriter);

            // Copy File (Local) To Directory (S3)
            //String sourceFile = @"C:\Temp\Source\YamlDotNet.xml";
            //String targetDir = @"s3://wagug0-test/Destination/";
            //WindowsSynapseFile source = new WindowsSynapseFile(sourceFile);
            //AwsS3SynapseDirectory target = new AwsS3SynapseDirectory(targetDir);
            //source.CopyTo(target, true, true, "Sandbox", ConsoleWriter);

            //String sourceFile = @"s3://wagug0-test/Source/Synapseliverance.yaml";
            //String targetFile = @"s3://wagug0-test/Destination/Synapseliverance.yaml";

            //String sourceDir = @"s3://wagug0-test/Source/";
            //String targetDir = @"s3://wagug0-test/Destination/";


            //AwsS3SynapseDirectory synSourceDir = new AwsS3SynapseDirectory( sourceDir );
            //AwsS3SynapseDirectory synTargetDir = new AwsS3SynapseDirectory( targetDir );

            //synSourceDir.CopyTo(synTargetDir, true, true, "Sandbox", ConsoleWriter);

            //String path = synSourceDir.PathCombine( synTargetDir.FullName, "Guy/", "Was/", "Here/", "/", "Boo" );
            //Console.WriteLine( path );


            //Console.WriteLine("Directories : ");
            //foreach (SynapseDirectory dir in synSourceDir.GetDirectories())
            //    Console.WriteLine($">> {dir.FullName}");

            //Console.WriteLine( "Files : " );
            //foreach ( SynapseFile file in synSourceDir.GetFiles() )
            //    Console.WriteLine( $">> {file.FullName}" );


            //synTargetDir.Create();
            //synTargetDir.Create( @"s3://wagug0-test/Destination/TestFolder/" );
            //Console.WriteLine( ">> Exists : " + synTargetDir.Exists() );
            //Console.WriteLine( ">> Exists : " + synTargetDir.Exists( @"s3://wagug0-test/Destination/TestFolder/" ) );

            //synTargetDir.Delete( @"s3://wagug0-test/Destination/TestFolder/" );
            //synTargetDir.Delete();
            //Console.WriteLine( ">> Exists : " + synTargetDir.Exists() );
            //Console.WriteLine( ">> Exists : " + synTargetDir.Exists( @"s3://wagug0-test/Destination/TestFolder/" ) );

            //AwsS3SynapseFile synSourceFile = new AwsS3SynapseFile( sourceFile );
            //Stream stream = synSourceFile.OpenStream(AccessType.Read);
            //StreamReader reader = new StreamReader( stream );
            //String contents = reader.ReadToEnd();
            //Console.WriteLine( contents );
            //synSourceFile.CloseStream();

            //AwsS3SynapseFile synTargetFile = new AwsS3SynapseFile( targetFile );
            //stream = synTargetFile.OpenStream( AccessType.Write );
            //StreamWriter writer = new StreamWriter( stream );
            //writer.Write( contents );
            //synTargetFile.CloseStream();

            //synTargetFile.Create();
            //synTargetFile.Create( @"s3://wagug0-test/Destination/GuyTest.txt" );

            //synTargetFile.Delete( @"s3://wagug0-test/Destination/GuyTest.txt" );
            //synTargetFile.Delete();

            //String sourceFile = @"C:\Temp\Source\YamlDotNet.xml";
            //String targetFile = @"C:\Temp\Destination\YamlDotNet.xml";
            ////sourceFile = @"\\localhost\C$\Temp\Source\YamlDotNet.xml";
            ////targetFile = @"\\localhost\C$\Temp\Destination\YamlDotNet.xml";

            //WindowsSynapseFile synSourceFile = new WindowsSynapseFile( sourceFile );
            //WindowsSynapseFile synTargetFile = new WindowsSynapseFile( targetFile );
            //synSourceFile.CopyTo( synTargetFile );
            //synTargetFile.Delete();
            //synSourceFile.MoveTo( synTargetFile );

            //String sourceDir = @"C:\Temp\Source";
            //String targetDir = @"C:\Temp\Destination";
            //sourceDir = @"\\localhost\C$\Temp\Source";
            //targetDir = @"\\localhost\C$\Temp\Destination";

            //WindowsSynapseDirectory synSourceDir = new WindowsSynapseDirectory(sourceDir);
            //WindowsSynapseDirectory synTargetDir = new WindowsSynapseDirectory(targetDir);
            //synSourceDir.CopyTo(synTargetDir, true, true, "Sandbox", ConsoleWriter);
            //Console.WriteLine("================================");
            //synTargetDir.Clear(null, "Sandbox", ConsoleWriter);
            //Console.WriteLine("================================");
            //synSourceDir.MoveTo(synTargetDir, true, true, "Sandbox", ConsoleWriter);

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        private static void ConsoleWriter(string label, string message)
        {
            Console.WriteLine( $"{label} - {message}" );
        }
    }
}
