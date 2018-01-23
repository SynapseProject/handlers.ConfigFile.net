using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using Synapse.Core;

using YamlDotNet.Serialization;

using Synapse.Core.Utilities;

namespace Synapse.Handlers.FileUtil
{
    public class MungeFileHandlerConfig
    {
        [XmlElement]
        public ConfigType Type { get; set; } = ConfigType.None;
        [XmlElement]
        public bool BackupSource { get; set; } = false;
        [XmlElement]
        public bool CreateSettingIfNotFound { get; set; } = false;
        [XmlElement]
        public bool StopOnError { get; set; } = true;
        [XmlElement]
        public bool RunSequential { get; set; } = false;
        [XmlElement]
        public bool OverwriteExisting { get; set; } = true;
        [XmlElement]
        public AwsConfig Aws { get; set; }
    }

}
