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
        public bool Recursive { get; set; } = true;
        [XmlElement]
        public bool FailIfMissing { get; set; } = false;
        [XmlElement]
        public bool Verbose { get; set; } = true;
        [XmlElement]
        public AwsConfig Aws { get; set; }
    }

}
