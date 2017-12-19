using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using YamlDotNet.Serialization;
using Amazon;

namespace Synapse.Handlers.FileUtil
{
    public class AwsConfig
    {
        [XmlElement]
        public string AccessKey { get; set; }
        [XmlElement]
        public string SecretKey { get; set; }
        [XmlElement]
        public string Region
        {
            get
            {
                return AwsRegion.SystemName;
            }
            set
            {
                AwsRegion = RegionEndpoint.GetBySystemName(value);
            }
        }
        [XmlIgnore]
        [YamlIgnore]
        public RegionEndpoint AwsRegion { get; protected set; } = RegionEndpoint.EUWest1;
    }
}
