using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Synapse.Core;

namespace Synapse.Handlers.FileUtil
{
    public class CryptoUtils
    {
        public static String DecryptCsvFile(String filename, CryptoProvider crypto)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return DecryptCsvFile(stream, crypto);
        }

        public static String DecryptCsvFile(Stream file, CryptoProvider crypto)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader reader = new StreamReader(file))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    char[] delims = { ',' };
                    String[] values = line.Split(delims);
                    bool firstValue = true;
                    foreach (String value in values)
                    {
                        String newValue = null;
                        crypto.TryDecryptOrValue(value, out newValue);
                        if (firstValue)
                            firstValue = false;
                        else
                            sb.Append(",");

                        sb.Append(newValue);
                    }
                    sb.AppendLine(String.Empty);
                }
                reader.Close();
            }

            return sb.ToString();
        }

        public static String DecryptXmlFile(String filename, CryptoProvider crypto)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return DecryptXmlFile(stream, crypto);
        }

        public static String DecryptXmlFile(Stream file, CryptoProvider crypto)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            DecryptXmlNode(root, crypto);
            return doc.OuterXml;
        }

        private static void DecryptXmlNode(XmlNode node, CryptoProvider crypto)
        {
            if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Attribute)
            {
                String newValue = null;
                crypto.TryDecryptOrValue(node.Value, out newValue);
                if (node.Value != newValue)
                    node.Value = newValue;
            }
            else
            {
                foreach (XmlNode child in node.ChildNodes)
                    DecryptXmlNode(child, crypto);
                foreach (XmlNode attribute in node.Attributes)
                    DecryptXmlNode(attribute, crypto);
            }
        }

    }
}
