using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using YamlDotNet.Serialization;

using Synapse.Core.Utilities;

namespace Synapse.Handlers.FileUtil
{
    [XmlRoot(ElementName="Root")]
    public class CopyFileHandlerParameters
    {
        [XmlArrayItem("ElementName")]
        public List<FileSet> FileSets { get; set; }
    }

    public class FileSet
    {
        [XmlArrayItem(ElementName = "Source")]
        public List<String> Sources { get; set; }
        [XmlArrayItem(ElementName = "Destination")]
        public List<String> Destinations { get; set; }
    }
}
