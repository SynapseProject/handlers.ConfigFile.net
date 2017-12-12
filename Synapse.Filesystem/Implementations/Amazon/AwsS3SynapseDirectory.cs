using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Amazon.S3.IO;
using Amazon.S3.Model;

namespace Synapse.Filesystem
{
    public class AwsS3SynapseDirectory : SynapseDirectory
    {
        public static string UrlPattern = @"^(s3:\/\/)(.*?)\/(.*)$";        // Gets Root, Bucket Name and Object Key
        public static string NamePattern = @"^(s3:\/\/.*\/)(.*?)\/$";       // Gets Parent Name and Name

        private string _fullName;
        public string BucketName { get; internal set; }
        public string ObjectKey { get; internal set; }

        public override string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                Match match = Regex.Match( value, UrlPattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                {
                    BucketName = match.Groups[2].Value;
                    ObjectKey = match.Groups[3].Value;
                }
            }
        }

        public override string Name {
            get
            {
                String name = null;
                Match match = Regex.Match( FullName, NamePattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    name = match.Groups[2].Value;
                return name;
            }
        }
        public override string Parent {
            get
            {
                String parent = null;
                Match match = Regex.Match( FullName, NamePattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    parent = match.Groups[1].Value;
                return parent;
            }
        }
        public override string Root {
            get
            {
                String name = null;
                Match match = Regex.Match( FullName, UrlPattern, RegexOptions.IgnoreCase );
                if ( match.Success )
                    name = match.Groups[1].Value;
                return name;
            }
        }


        public AwsS3SynapseDirectory() { }
        public AwsS3SynapseDirectory(string fullName)
        {
            FullName = fullName;
        }



        public override SynapseDirectory Create(string childDirName = null, string callbackLabel = null, Action<string, string> callback = null)
        {
            if ( childDirName == null || childDirName == FullName )
            {
                String key = ObjectKey;
                if ( key.EndsWith( "/" ) )
                    key = key.Substring( 0, key.Length - 1 );
                S3DirectoryInfo dirInfo = new S3DirectoryInfo( AwsClient.Client, BucketName, key);
                dirInfo.Create();
                callback?.Invoke( callbackLabel, $"Directory [{FullName}] Was Created." );
                return this;
            }
            else
            {
                AwsS3SynapseDirectory newDir = new AwsS3SynapseDirectory( childDirName );
                newDir.Create();
                return newDir;
            }
        }

        public override void Delete(string dirName = null, string callbackLabel = null, Action<string, string> callback = null)
        {
            if (dirName == null || dirName == FullName)
            {
                S3DirectoryInfo dirInfo = new S3DirectoryInfo( AwsClient.Client, BucketName, ObjectKey );
                dirInfo.Delete( true );
                callback?.Invoke( callbackLabel, $"Directory [{FullName}] Was Deleted." );
            }
            else
            {
                AwsS3SynapseDirectory newDir = new AwsS3SynapseDirectory( dirName );
                newDir.Delete();
            }
        }

        public override bool Exists(string dirName = null)
        {
            if ( dirName == null || dirName == FullName )
            {
                S3DirectoryInfo dirInfo = new S3DirectoryInfo( AwsClient.Client, BucketName, ObjectKey );
                return dirInfo.Exists;
            }
            else
            {
                AwsS3SynapseDirectory newDir = new AwsS3SynapseDirectory( dirName );
                return newDir.Exists();
            }
        }

        public override IEnumerable<SynapseDirectory> GetDirectories()
        {
            List<SynapseDirectory> dirs = new List<SynapseDirectory>();
            List<S3Object> objects = GetObjects( BucketName, ObjectKey );

            foreach ( S3Object obj in objects )
                if ( obj.Key.EndsWith( @"/" ) )
                {
                    String dirName = obj.Key.Replace( ObjectKey, "" );
                    // Exclude Sub-Directories
                    if ( dirName.Split( new char[] { '/' } ).Length == 2 )
                        dirs.Add( new AwsS3SynapseDirectory( $"s3://{obj.BucketName}/{obj.Key}" ) );
                }

            return dirs;
        }

        public override IEnumerable<SynapseFile> GetFiles()
        {
            List<SynapseFile> files = new List<SynapseFile>();
            List<S3Object> objects = GetObjects( BucketName, ObjectKey );

            foreach ( S3Object obj in objects )
                if ( !obj.Key.EndsWith( @"/" ) )
                {
                    String fileName = obj.Key.Replace( ObjectKey, "" );
                    // Exclude Sub-Directories
                    if ( fileName.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries ).Length == 1 )
                        files.Add( new AwsS3SynapseFile( $"s3://{obj.BucketName}/{obj.Key}" ) );
                }

            return files;
        }

        public override string PathCombine(params string[] paths)
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<paths.Length; i++)
            {
                string path = paths[i]?.Trim();
                if ( path == null )
                    continue;
                else if ( path.EndsWith( "/" ) )
                    sb.Append( path );
                else if ( i == paths.Length - 1)
                    sb.Append( path );
                else
                    sb.Append( $"{path}/" );
            }

            return sb.ToString();
        }

        private List<S3Object> GetObjects(string bucketName, string prefix = null)
        {
            ListObjectsV2Request request = new ListObjectsV2Request();
            request.BucketName = bucketName;
            if ( prefix != null )
                request.Prefix = prefix;

            ListObjectsV2Response response = AwsClient.Client.ListObjectsV2( request );
            return response.S3Objects;
        }
    }
}
