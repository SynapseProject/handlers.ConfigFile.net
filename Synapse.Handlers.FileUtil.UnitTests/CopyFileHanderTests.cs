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

        [OneTimeSetUp]
        public void Init()
        {
            _root = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            Directory.SetCurrentDirectory( $@"{_root}\..\.." );
            _root = Directory.GetCurrentDirectory();
            _plansRoot = $@"{_root}\Plans\CopyFile";
            _inputFiles = $@"{_plansRoot}\InputFiles";
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
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";

            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";
            string destDir2 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\File1.txt
        Destinations:
        - {destDir1}\File1New.txt
        - {destDir2}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{destDir1}\File1New.txt" ) );
                Assert.True( File.Exists( $@"{destDir2}\File1New.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void CopyFileToDir()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";
            string destDir2 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\File1.txt
        Destinations:
        - {destDir1}\
        - {destDir2}\";

            Console.WriteLine( planInput );
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( File.Exists( $@"{sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{destDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{destDir2}\File1.txt" ) );
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( 1, countDest1 );
                Assert.AreEqual( 1, countDest2 );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void CopyDirToDir()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string sourceDir2 = $@"{workingDirectory}\Dir2";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";
            string destDir2 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\
        - {sourceDir2}\
        Destinations:
        - {destDir1}\
        - {destDir2}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{sourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{sourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
                int countDest1 = Directory.EnumerateFileSystemEntries( $@"{destDir1}", "*", SearchOption.AllDirectories ).Count();
                int countDest2 = Directory.EnumerateFileSystemEntries( $@"{destDir2}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest1 );
                Assert.AreEqual( countSource, countDest2 );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void CopyDirNoRecurseShouldCopySubDirsButNotContents()
        {
            
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir2 = $@"{workingDirectory}\Dir2";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir2}\
        Destinations:
        - {destDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );

                int countSource = Directory.EnumerateFileSystemEntries( $@"{sourceDir2}", "*", SearchOption.TopDirectoryOnly ).Count();
                int countDest = Directory.EnumerateFileSystemEntries( $@"{destDir1}", "*", SearchOption.AllDirectories ).Count();
                Assert.AreEqual( countSource, countDest );
            }
            Utilities.CleanupTestFiles( workingDirectory );
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
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\File1.txt
        Destinations:
        - {destDir1}\File1New.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{destDir1}\File1New.txt" ) );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void MoveFileToDir()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\File1.txt
        Destinations:
        - {destDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{sourceDir1}\File1.txt" ) );
                Assert.True( File.Exists( $@"{destDir1}\File1.txt" ) );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void MoveDirToDir()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string sourceDir2 = $@"{workingDirectory}\Dir2";
            string destDir1 = $@"{workingDirectory}\{Path.GetRandomFileName().Replace( ".", "" )}";

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
        - {sourceDir1}\
        - {sourceDir2}\
        Destinations:
        - {destDir1}\";

            Console.WriteLine( planInput );

            int countSource = Directory.EnumerateFileSystemEntries( $@"{sourceDir1}", "*", SearchOption.AllDirectories ).Count() +
                    Directory.EnumerateFileSystemEntries( $@"{sourceDir2}", "*", SearchOption.AllDirectories ).Count(); ;
            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                int countDest = Directory.EnumerateFileSystemEntries( $@"{destDir1}", "*", SearchOption.AllDirectories ).Count();

                Assert.AreEqual( countSource, countDest );
            }
            Utilities.CleanupTestFiles( workingDirectory );
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
