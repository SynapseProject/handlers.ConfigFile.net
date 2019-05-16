using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core;
using System.IO;
using Synapse.Core.Utilities;

namespace Synapse.Handlers.FileUtil.UnitTests
{
    [TestFixture]
    public class MungeFileHandlerTests
    {
        public static string _root = null;
        public static string _plansRoot = null;
        public static string _inputFiles = null;
        public static string _outputFiles = null;

        [OneTimeSetUp]
        public void Init()
        {
            _root = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            Directory.SetCurrentDirectory( $@"{_root}\..\.." );
            _root = Directory.GetCurrentDirectory();
            _plansRoot = $@"{_root}\Plans\MungeFile";
            _inputFiles = $@"{_plansRoot}\InputFiles";
            _outputFiles = $@"{_plansRoot}\OutputFiles";
        }
        [Test]
        public void S3EndPointsWithoutAwsConfigSectionShouldFail()
        {
            Plan plan = Plan.FromYaml( $@"{_plansRoot}\s3-end-point-without-aws-config-section1.yaml" );
            plan.Start( null, false, true );
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );

            plan = Plan.FromYaml( $@"{_plansRoot}\s3-end-point-without-aws-config-section2.yaml" );
            plan.Start( null, false, true );
            status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );

            plan = Plan.FromYaml( $@"{_plansRoot}\s3-end-point-without-aws-config-section3.yaml" );
            plan.Start( null, false, true );
            status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Failed.ToString(), status );
        }
        [Test]
        [TestCase( "dont-create-new-settings" )]
        [TestCase( "without-settings-file" )]
        [TestCase( "with-settings-file" )]
        [TestCase( "modify-in-place" )]
        [TestCase( "crypto" )]
        public void ModifyFile(string planFileWithoutExtension)
        {
            string workingDirectory = $@"{_root}\temp_{Path.GetRandomFileName().Replace( ".", "" )}";
            Utilities.SetupTestFiles( _inputFiles, workingDirectory );

            Plan plan = Plan.FromYaml( $@"{_plansRoot}\{planFileWithoutExtension}.yaml" );

            string parmString = plan.Actions[0].Parameters.GetSerializedValues();
            string newParmString = parmString.Replace( "$WORKING_DIRECTORY", workingDirectory );
            plan.Actions[0].Parameters.Values = YamlHelpers.Deserialize<Dictionary<object, object>>( newParmString );

            if (plan.Actions[0].Parameters.HasCrypto)
            {
                string cyptoConfigString = plan.Actions[0].Parameters.Crypto.Provider.Config.GetSerializedValues();
                string newCryptoConfigString = cyptoConfigString.Replace( "$WORKING_DIRECTORY", workingDirectory );
                plan.Actions[0].Parameters.Crypto.Provider.Config.Values = YamlHelpers.Deserialize<Dictionary<object, object>>( newCryptoConfigString );
            }
            plan.Start( null, false, true );

            // expected results file: {type}.out
            string status = plan.ResultPlan.Actions[0].Result.Status.ToString();
            Assert.AreEqual( StatusType.Success.ToString(), status );

            string configstring = plan.Actions[0].Handler.Config.GetSerializedValues();
            MungeFileHandlerConfig config = YamlHelpers.Deserialize<MungeFileHandlerConfig>( configstring );

            parmString = plan.Actions[0].Parameters.GetSerializedValues();
            MungeFileHandlerParameters parms = YamlHelpers.Deserialize<MungeFileHandlerParameters>( parmString );
             
            foreach( ModifyFileType file in parms.Files )
            {
                string actualResult = File.ReadAllText( string.IsNullOrWhiteSpace( file.Destination ) ? file.Source: file.Destination );

                ConfigType modifyType = config.Type;
                if( file.Type != ConfigType.None )
                    modifyType = file.Type;

                string expectedResult = null;
                if (config.CreateSettingIfNotFound)
                    expectedResult = File.ReadAllText( $@"{_outputFiles}\{modifyType}-expected.out" );
                else
                    expectedResult = File.ReadAllText( $@"{_outputFiles}\{modifyType}-nonewsettings-expected.out" );
                Assert.AreEqual( expectedResult, actualResult );

            }
            Utilities.CleanupTestFiles( workingDirectory );
        }
        [Test]
        public void ModifyS3File()
        {
            throw new Exception( "There are no unit tests created for Aws S3 buckets" );
        }
    }
}
