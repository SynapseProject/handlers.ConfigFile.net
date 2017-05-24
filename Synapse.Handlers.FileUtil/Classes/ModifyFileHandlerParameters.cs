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
    public class ModifyFileHandlerParameters
    {
        [XmlArrayItem(ElementName="File")]
        public List<ModifyFileType> Files { get; set; }
    }

    public class ModifyFileType
    {
        [XmlElement]
        public String Source { get; set; }
        [XmlElement]
        public String Destination { get; set; }
        [XmlElement]
        public String SettingsFile { get; set; }
        [XmlElement]
        public bool? CreateSettingIfNotFound { get; set; } = null;
        [XmlArrayItem(ElementName = "Setting")]
        public List<SettingsPair<String, String>> Settings { get; set; }

        public List<KeyValuePair<String, String>> SettingsKvp { get { return GetSettingsKVP(); } }

        private List<KeyValuePair<String, String>> GetSettingsKVP()
        {
            List<KeyValuePair<String, String>> kvp = new List<KeyValuePair<String, String>>();
            foreach (SettingsPair<String, String> setting in this.Settings)
                kvp.Add(setting);

            return kvp;
        }
    }

    public struct SettingsPair<TKey, TValue>
    {
        [XmlElement]
        public TKey Key { get; set; }
        [XmlElement]
        public TValue Value { get; set; }

        public SettingsPair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        static public implicit operator KeyValuePair<TKey, TValue>(SettingsPair<TKey, TValue> pair)
        {
            return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
        }
            

    }
}
