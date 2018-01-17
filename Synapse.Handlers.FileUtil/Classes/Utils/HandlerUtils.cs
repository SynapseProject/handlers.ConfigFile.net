using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Zephyr.Filesystem;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.Handlers.FileUtil
{
    public static class HandlerUtils
    {
        public static T Deserialize<T>(String str)
        {
            T obj;
            if (str.Trim().StartsWith("<"))
                try
                {
                    obj = XmlHelpers.Deserialize<T>(new StringReader(str));
                }
                catch (Exception e)
                {
                    // Check Edge Case Of Yaml Document Starting With A "<" Character
                    try
                    {
                        obj = YamlHelpers.Deserialize<T>(new StringReader(str));
                    }
                    catch (Exception)
                    {
                        throw e;
                    }
                }
            else
                obj = YamlHelpers.Deserialize<T>(new StringReader(str));

            return obj;
        }

        public static String Serialize<T>(object obj)
        {
            String str = String.Empty;

            if (obj.GetType() == typeof(XmlNode[]))
            {
                StringBuilder sb = new StringBuilder();
                String type = typeof(T).Name;
                sb.Append("<" + type + ">");
                XmlNode[] nodes = (XmlNode[])obj;
                foreach (XmlNode node in nodes)
                    sb.Append(node.OuterXml);
                sb.Append("</" + type + ">");
                str = sb.ToString();
            }
            else if (obj.GetType() == typeof(Dictionary<object, object>))
                str = YamlHelpers.Serialize(obj);
            else
                str = obj.ToString();

            return str;
        }

        public static double ElapsedSeconds(this Stopwatch stopwatch)
        {
            return TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds;
        }


        public static String Base64Encode(String str)
        {
            String encodedStr = null;
            if (str != null)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(str);
                encodedStr = Convert.ToBase64String(bytes);
            }

            return encodedStr;
        }

        public static String Base64Decode(String encodedStr)
        {
            String decodedStr = null;
            if (encodedStr != null)
            {
                byte[] data = Convert.FromBase64String(encodedStr);
                decodedStr = Encoding.UTF8.GetString(data);
            }

            return decodedStr;
        }

        public static AwsClient InitAwsClient(AwsConfig aws)
        {
            AwsClient client = null;
            if (aws != null)
            {
                bool hasAccessKey = (!String.IsNullOrWhiteSpace(aws.AccessKey));
                bool hasSecretKey = (!String.IsNullOrWhiteSpace(aws.SecretKey));
                bool hasRegion = (!String.IsNullOrWhiteSpace(aws.Region));

                if (hasAccessKey && hasSecretKey)
                {
                    if (hasRegion)
                        client = new AwsClient(aws.AccessKey, aws.SecretKey, aws.AwsRegion);
                    else
                        client = new AwsClient(aws.AccessKey, aws.SecretKey);
                }
                else if (hasRegion)
                    client = new AwsClient(aws.AwsRegion);
                else
                    client = new AwsClient();     // Pull All Details From Environemnt Variables / Credentails Files
            }
            return client;
        }


    }
}
