using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphaleonis.Win32.Filesystem;

namespace Synapse.Filesystem
{
    public static class Utilities
    {
        public static UrlType GetUrlType(string url)
        {
            UrlType type = UrlType.Unknown;

            if (url.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
            {
                if (IsDirectory(url))
                    type = UrlType.AwsS3Directory;
                else
                    type = UrlType.AwsS3File;
            }
            else if (url.StartsWith("\\"))
            {
                if (IsDirectory(url))
                    type = UrlType.NetworkDirectory;
                else
                    type = UrlType.NetworkFile;
            }
            else
            {
                if (IsDirectory(url))
                    type = UrlType.LocalDirectory;
                else
                    type = UrlType.LocalFile;
            }

            return type;
        }

        public static bool IsDirectory(string url)
        {
            return (url.EndsWith("/") || url.EndsWith(@"\"));
        }

        public static bool IsFile(string url)
        {
            return !IsDirectory(url);
        }

        public static SynapseFile GetSynapseFile(string url)
        {
            SynapseFile file = null;
            UrlType type = GetUrlType(url);
            switch (type)
            {
                case UrlType.LocalFile:
                    file = new WindowsSynapseFile(url);
                    break;
                case UrlType.NetworkFile:
                    file = new WindowsSynapseFile(url);
                    break;
                case UrlType.AwsS3File:
                    file = new AwsS3SynapseFile(url);
                    break;
                default:
                    throw new Exception($"Url [{url}] Is Not A Known File Type.");
            }

            return file;
        }

        public static SynapseDirectory GetSynapseDirectory(string url)
        {
            SynapseDirectory dir = null;
            UrlType type = GetUrlType(url);
            switch (type)
            {
                case UrlType.LocalDirectory:
                    dir = new WindowsSynapseDirectory(url);
                    break;
                case UrlType.NetworkDirectory:
                    dir = new WindowsSynapseDirectory(url);
                    break;
                case UrlType.AwsS3Directory:
                    dir = new AwsS3SynapseDirectory(url);
                    break;
                default:
                    throw new Exception($"Url [{url}] Is Not A Known Directory Type.");
            }

            return dir;
        }
    }
}
