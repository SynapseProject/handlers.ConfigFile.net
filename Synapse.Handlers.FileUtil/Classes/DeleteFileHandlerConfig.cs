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
    public class DeleteFileHandlerConfig
    {
        [XmlElement]
        public bool Recurse { get; set; } = true;
        [XmlElement]
        public bool StopOnError { get; set; } = true;
        [XmlElement]
        public bool Verbose { get; set; } = true;
        [XmlElement]
        public AwsConfig Aws { get; set; }
    }

}
