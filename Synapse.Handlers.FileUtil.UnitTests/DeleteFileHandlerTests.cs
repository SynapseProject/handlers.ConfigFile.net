using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core;
using System.IO;

namespace Synapse.Handlers.FileUtil.UnitTests
{
    [TestFixture]
    public class DeleteFileHandlerTests
    {
        const string _planFolderName = "DeleteHandler";

        [Test]
        public void S3EndPointsWithoutAwsConfigSectionShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{Global.PlansRoot}\{_planFolderName}\s3-end-point-without-aws-config-section.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        public void DeleteFile()
        {
            Global.SetupTestFiles();

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
      - {Global.SourceDir1}\File1.txt
      - {Global.SourceDir2}\File2.txt";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !File.Exists( $@"{Global.SourceDir1}\File1.txt" ) );
                Assert.True( !File.Exists( $@"{Global.SourceDir2}\File2.txt" ) );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        public void DeleteDir()
        {
            Global.SetupTestFiles();

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
      - {Global.SourceDir1}\
      - {Global.SourceDir2}\";

            Console.WriteLine( planInput );

            using( TextReader reader = new StringReader( planInput ) )
            {
                Plan plan = Plan.FromYaml( reader );
                plan.Start( null, false, true );
                string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
                Assert.AreEqual( StatusType.Success.ToString(), status );
                Assert.True( !Directory.Exists( $@"{Global.SourceDir1}" ) );
                Assert.True( !Directory.Exists( $@"{Global.SourceDir2}" ) );
            }
            Global.CleanupTestFiles();
        }
        [Test]
        public void DeleteS3File()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
        [Test]
        public void DeleteS3Dir()
        {
            throw new Exception( "There are no unit tests created for the Aws S3 buckets" );
        }
    }
}
