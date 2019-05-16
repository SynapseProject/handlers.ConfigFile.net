using NUnit.Framework;
using System;
using Synapse.Core;
using System.IO;

namespace Synapse.Handlers.FileUtil.UnitTests
{
    [TestFixture]
    public class DeleteFileHandlerTests
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
            _plansRoot = $@"{_root}\Plans\DeleteFile";
            _inputFiles = $@"{_root}\Plans\CopyFile\InputFiles";  // shares the same input files as copy file handler
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
        public void DeleteFile()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string sourceDir2 = $@"{workingDirectory}\Dir2";

            string planInput = $@"Name: SamplePlan
Description: Test DeleteFile handler
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:DeleteFileHandler
    Config:
      Values:
        Recurse: true
        StopOnError: true
        Verbose: true
  Parameters:
    Values:
      Targets:
      - {sourceDir1}\File1.txt
      - {sourceDir2}\File2.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{sourceDir1}\File1.txt" ) );
                Assert.True( !File.Exists( $@"{sourceDir2}\File2.txt" ) );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void DeleteDir()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";
            string sourceDir2 = $@"{workingDirectory}\Dir2";

            string planInput = $@"Name: SamplePlan
Description: Test DeleteFile handler
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:DeleteFileHandler
    Config:
      Values:
        Recurse: true
        StopOnError: true
        Verbose: true
  Parameters:
    Values:
      Targets:
      - {sourceDir1}\
      - {sourceDir2}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !Directory.Exists( $@"{sourceDir1}" ) );
                Assert.True( !Directory.Exists( $@"{sourceDir2}" ) );
            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void DeleteDirNoRecurseRequiresDirToBeEmpty()
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );
            string sourceDir1 = $@"{workingDirectory}\Dir1";

            string planInput = $@"Name: SamplePlan
Description: Test DeleteFile handler
Actions:
- Name: Action001
  Handler:
    Type: Synapse.Handlers.FileUtil:DeleteFileHandler
    Config:
      Values:
        Recurse: false
        StopOnError: true
        Verbose: true
  Parameters:
    Values:
      Targets:
      - {sourceDir1}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Failed.ToString(), status );

                // empty directory
                DirectoryInfo dirInfo = new DirectoryInfo( sourceDir1 );
                foreach( var file in dirInfo.GetFiles( "*", SearchOption.TopDirectoryOnly ) )
                    file.Delete();
                foreach( var dir in dirInfo.GetDirectories( "*", SearchOption.TopDirectoryOnly ) )
                    dir.Delete( true );

                
                    //plan = Plan.FromYaml( reader );
                    plan.Start( null, false, true );
                    status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                    Assert.AreEqual( StatusType.Success.ToString(), status );
                
            }

            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void DeleteS3File()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
        [Test]
        public void DeleteS3Dir()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
    }
}
