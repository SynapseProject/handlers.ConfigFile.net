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
    [XmlRoot(ElementName="Root")]
    public class CopyFileHandlerConfig
    {
        [XmlElement]
        public FileAction Action { get; set; } = FileAction.Copy;
        [XmlElement]
        public bool OverwriteExisting { get; set; } = true;
        [XmlElement]
        public bool IncludeSubdirectories { get; set; } = true;
        [XmlElement]
        public bool MaintainAttributes { get; set; } = true;
        [XmlElement]
        public bool PurgeDestination { get; set; } = false;
    }

}
