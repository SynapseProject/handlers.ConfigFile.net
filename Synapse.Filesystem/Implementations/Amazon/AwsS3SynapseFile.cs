using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.IO;
using Amazon.S3.Transfer;

namespace Synapse.Filesystem
{
    public class AwsS3SynapseFile : SynapseFile
    {
        public static string UrlPattern = @"^(s3:\/\/)(.*?)\/(.*)$";        // Gets Root, Bucket Name and Object Key

        private System.IO.Stream fileStream;
        private bool isStreamOpen = false;

        public override string Name { get { return FullName.Substring( FullName.LastIndexOf( @"/" ) + 1 ); } }
        public override string FullName {
            get { return _fullName; }
            set
            {
                _fullName = value;
                Match match = Regex.Match( value, UrlPattern, RegexOptions.IgnoreCase );
                if (match.Success)
                {
                    BucketName = match.Groups[2].Value;
                    ObjectKey = match.Groups[3].Value;
                }
            }
        }

        private string _fullName;

        public string BucketName { get; internal set; }
        public string ObjectKey { get; internal set; }

        public AwsS3SynapseFile() { }
        public AwsS3SynapseFile(string fullName)
        {
            FullName = fullName;
        }


        public override System.IO.Stream OpenStream(AccessType access, string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( !isStreamOpen )
            {
                S3FileInfo file = new S3FileInfo( AwsClient.Client, BucketName, ObjectKey );
                if ( access == AccessType.Read )
                    fileStream = file.OpenRead();
                else if ( access == AccessType.Write )
                    fileStream = file.OpenWrite();
                else
                    throw new Exception( $"Unknown AccessType [{access}] Received." );
                isStreamOpen = true;
            }
            return fileStream;
        }

        public override void CloseStream(string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( isStreamOpen )
            {
                fileStream.Close();
                isStreamOpen = false;
            }
        }

        public override SynapseFile Create(string fileName = null, bool overwrite = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName )
            {
                try
                {
                    if (this.Exists() && !overwrite)
                        throw new Exception($"File [{this.FullName}] Already Exists.");

                    S3FileInfo fileInfo = new S3FileInfo(AwsClient.Client, BucketName, ObjectKey);
                    fileStream = fileInfo.Create();
                    isStreamOpen = true;            // Opens Stream As "Write" By Default
                    callback?.Invoke(callbackLabel, $"File [{FullName}] Was Created.");
                    return this;
                }
                catch (Exception e)
                {
                    Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                    throw;

                }
            }
            else
            {
                AwsS3SynapseFile file = new AwsS3SynapseFile( fileName );
                file.Create( null, overwrite );
                return file;
            }
        }

        public override SynapseDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null)
        {
            return new AwsS3SynapseDirectory(dirName);
        }

        public override void Delete(string fileName = null, bool verbose = true, string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( fileName == null || fileName == FullName )
            {
                S3FileInfo fileInfo = new S3FileInfo( AwsClient.Client, BucketName, ObjectKey );
                fileInfo.Delete();
                if (verbose)
                    Logger.Log($"File [{FullName}] Was Deleted.", callbackLabel, callback);
            }
            else
            {
                AwsS3SynapseFile file = new AwsS3SynapseFile( fileName );
                file.Delete();
            }
        }

        public override bool Exists(string fileName = null)
        {
            if (fileName == null || fileName == FullName)
            {
                String key = ObjectKey;
                key = key.Replace('/', '\\');
                S3FileInfo fileInfo = new S3FileInfo( AwsClient.Client, BucketName, key );
                return fileInfo.Exists;
            }
            else
            {
                AwsS3SynapseFile file = new AwsS3SynapseFile( fileName );
                return file.Exists();
            }
        }

    }
}
