// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Linq;
using NUnit.Framework;
using System.IO;
using Synapse.Core;
using System;

namespace Synapse.Handlers.FileUtil.UnitTests
{
    [TestFixture]
    public class CopyFileHanderTests
    {

        public static string _root = null;
        public static string _plansRoot = null;
        public static string _inputFiles = null;
        public static string _workingDirectory = null;
        public static string _sourceDir1 = null;
        public static string _sourceDir2 = null;
        public static string _destDir1 = null;
        public static string _destDir2 = null;

        [OneTimeSetUp]
        public void Init()
        {
            _root = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            Directory.SetCurrentDirectory( $@"{_root}\..\.." );
            _root = Directory.GetCurrentDirectory();
            _plansRoot = $@"{_root}\Plans\CopyFile";
            _inputFiles = $@"{_plansRoot}\InputFiles";
            _workingDirectory = $@"{_root}\WorkingDir";
            _sourceDir1 = $@"{_workingDirectory}\Dir1";
            _sourceDir2 = $@"{_workingDirectory}\Dir2";
            _destDir1 = $@"{_workingDirectory}\Dest1";
            _destDir2 = $@"{_workingDirectory}\Dest2";
        }
        [Test]
        public void InvalidSourceShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{_plansRoot}\invalid-source.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        public void CopyDirectoryToFileShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{_plansRoot}\copy-dir-to-file.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        public void MoveToMultipleDestinationsShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{_plansRoot}\move-to-multiple-dest.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        public void S3EndPointsWithoutAwsConfigSectionShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{_plansRoot}\s3-end-point-without-aws-config-section.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        public void CopyFileToFile()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );

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
        - {_sourceDir1}\File1.txt
        Destinations:
        - {_destDir1}\File1New.txt
        - {_destDir2}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{_sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{_destDir1}\File1New.txt" ) );
                Assert.True( File.Exists( $@"{_destDir2}\File1New.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{_destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{_destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void CopyFileToDir()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );

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
        - {_sourceDir1}\File1.txt
        Destinations:
        - {_destDir1}\
        - {_destDir2}\";

            Console.WriteLine( planInput );
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{_sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{_destDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{_destDir2}\File1.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{_destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{_destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void CopyDirToDir()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );
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
        - {_sourceDir1}\
        - {_sourceDir2}\
        Destinations:
        - {_destDir1}\
        - {_destDir2}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{_sourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{_sourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{_destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{_destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest1 );
                Assert.AreEqual( countSource, countDest2 );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void CopyDirNoRecurseShouldCopySubDirsButNotContents()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );
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
        - {_sourceDir2}\
        Destinations:
        - {_destDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{_sourceDir2}", "*", SearchOption.TopDirectoryOnly ).Count();
                int countDest = Directory.EnumerateFileSystemEntries( $@"{_destDir1}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void CopyS3FileToS3File()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void CopyS3FileToS3Dir()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void CopyS3DirToS3Dir()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void MoveFileToFile()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );

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
        - {_sourceDir1}\File1.txt
        Destinations:
        - {_destDir1}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{_sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{_destDir1}\File1New.txt" ) );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void MoveFileToDir()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );
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
        - {_sourceDir1}\File1.txt
        Destinations:
        - {_destDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{_sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{_destDir1}\File1.txt" ) );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void MoveDirToDir()
        {
            Utilities.SetupTestFiles( _inputFiles, _workingDirectory );
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
        - {_sourceDir1}\
        - {_sourceDir2}\
        Destinations:
        - {_destDir1}\";

            Console.WriteLine( planInput );

            int countSource = Directory.EnumerateFileSystemEntries( $@"{_sourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{_sourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                int countDest = Directory.EnumerateFileSystemEntries( $@"{_destDir1}", "*", SearchOption.AllDirectories ).Count();

                Assert.AreEqual( countSource, countDest );
            }
            Utilities.CleanupTestFiles( _workingDirectory );
        }
        [Test]
        public void MoveS3FileToS3File()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void MoveS3FileToS3Dir()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void MoveS3DirToS3Dir()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
    }
}
