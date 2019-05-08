// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using System.IO;
using Synapse.Core;
using Synapse.Core.Utilities;
using System;
namespace Synapse.Handlers.FileUtil.UnitTests
{
    [TestFixture]
    public class CopyFileHanderTests
    {
        const string _planFolderName = "CopyHandler";
        [Test]
        [Category("Validate")]
        public void InvalidSourceShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{Global.PlansRoot}\{_planFolderName}\invalid-source.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        [Category( "Validate" )]
        public void CopyDirectoryToFileShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{Global.PlansRoot}\{_planFolderName}\copy-dir-to-file.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        [Category( "Validate" )]
        public void MoveToMultipleDestinationsShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{Global.PlansRoot}\{_planFolderName}\move-to-multiple-dest.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        [Category( "Validate" )]
        public void S3EndPointsWithoutAwsConfigSectionShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{Global.PlansRoot}\{_planFolderName}\s3-end-point-without-aws-config-section.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        [Category( "Copy" )]
        public void CopyFileToFile()
        {
            Global.SetupTestFiles();

            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Copy
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\File1.txt
        Destinations:
        - {Global.DestDir1}\File1New.txt
        - {Global.DestDir2}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{Global.SourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir1}\File1New.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir2}\File1New.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Copy" )]
        public void CopyFileToDir()
        {
            Global.SetupTestFiles();

            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Copy
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\File1.txt
        Destinations:
        - {Global.DestDir1}\
        - {Global.DestDir2}\";

            Console.WriteLine( planInput );
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{Global.SourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir2}\File1.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Copy" )]
        public void CopyDirToDir()
        {
            Global.SetupTestFiles();
            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Copy
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\
        - {Global.SourceDir2}\
        Destinations:
        - {Global.DestDir1}\
        - {Global.DestDir2}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{Global.SourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{Global.SourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest1 );
                Assert.AreEqual( countSource, countDest2 );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Copy" )]
        public void CopyDirNoRecurseShouldCopySubDirsButNotContents()
        {
            Global.SetupTestFiles();
            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Copy
        OverwriteExisting: true
        Recurse: false
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir2}\
        Destinations:
        - {Global.DestDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{Global.SourceDir2}", "*", SearchOption.TopDirectoryOnly ).Count();
                int countDest = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir1}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Copy" )]
        public void CopyS3FileToS3File()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        [Category( "Copy" )]
        public void CopyS3FileToS3Dir()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        [Category( "Copy" )]
        public void CopyS3DirToS3Dir()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        [Category( "Move" )]
        public void MoveFileToFile()
        {
            Global.SetupTestFiles();

            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Move
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\File1.txt
        Destinations:
        - {Global.DestDir1}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{Global.SourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir1}\File1New.txt" ) );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Move" )]
        public void MoveFileToDir()
        {
            Global.SetupTestFiles();
            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Move
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\File1.txt
        Destinations:
        - {Global.DestDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{Global.SourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{Global.DestDir1}\File1.txt" ) );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        [Category( "Move" )]
        public void MoveDirToDir()
        {
            Global.SetupTestFiles();
            string planInput = $@"Name: SamplePlan
Description: Test CopyFile Handler
IsActive: true
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:CopyFileHandler
    Config:
      Type: Yaml
      Values:
        Action: Move
        OverwriteExisting: true
        Recurse: true
        PurgeDestination: false
        Verbose: true
  Parameters:
    Type: Yaml
    Values:
      FileSets:
      - Sources: 
        - {Global.SourceDir1}\
        - {Global.SourceDir2}\
        Destinations:
        - {Global.DestDir1}\";

            Console.WriteLine( planInput );

            int countSource = Directory.EnumerateFileSystemEntries( $@"{Global.SourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{Global.SourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                int countDest = Directory.EnumerateFileSystemEntries( $@"{Global.DestDir1}", "*", SearchOption.AllDirectories ).Count();

                Assert.AreEqual( countSource, countDest );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        public void MoveS3FileToS3File()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        public void MoveS3FileToS3Dir()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        public void MoveS3DirToS3Dir()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
    }
}
