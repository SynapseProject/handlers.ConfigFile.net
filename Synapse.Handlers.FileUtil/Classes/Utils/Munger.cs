using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.XmlTransform;
using Alphaleonis.Win32.Filesystem;
using io = System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Synapse.Handlers.FileUtil
{
    class Munger
    {
        static public void XMLTransform(String sourceFile, String destinationFile, String transformFile)
        {
            io.Stream transformStream = null;
            if (!String.IsNullOrWhiteSpace(transformFile))
                transformStream = new io.FileStream(transformFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            XMLTransform(sourceFile, destinationFile, transformStream);
        }

        static public void XMLTransform(String sourceFile, String destinationFile, io.Stream transformFile)
        {
            String outFile = destinationFile;
            if (String.IsNullOrWhiteSpace(outFile))
                outFile = sourceFile;

            using (XmlTransformableDocument doc = new XmlTransformableDocument())
            {
                doc.PreserveWhitespace = true;

                using (io.StreamReader sr = new io.StreamReader(sourceFile))
                {
                    doc.Load(sr);
                }

                using (XmlTransformation xt = new XmlTransformation(transformFile, null))
                {
                    xt.Apply(doc);
                    doc.Save(outFile);
                }
            }
        }

        static public void KeyValue(PropertyFile.Type type, String sourceFile, String destinationFile, String transformFile, List<KeyValuePair<String, String>> settings, bool createIfNotFound = false)
        {
            io.Stream transformStream = null;
            if (!String.IsNullOrWhiteSpace(transformFile))
                transformStream = new io.FileStream(transformFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            KeyValue(type, sourceFile, destinationFile, transformStream, settings, createIfNotFound);
        }


        static public void KeyValue(PropertyFile.Type type, String sourceFile, String destinationFile, io.Stream transformFile, List<KeyValuePair<String, String>> settings, bool createIfNotFound = false)
        {
            PropertyFile props = new PropertyFile(type, sourceFile);

            if (transformFile != null)
            {
                if (transformFile != null)
                {
                    using (io.StreamReader reader = new io.StreamReader(transformFile))
                    {
                        String line; 
                        while ((line = reader.ReadLine()) != null)
                        {
                            char[] delims = { ',' };
                            String[] values = line.Split(delims);

                            String section = null;
                            String key = null;
                            String value = null;

                            if (values.Length == 2)
                            {
                                key = values[0].Trim();
                                value = values[1].Trim();
                            }
                            else if (values.Length >= 3)
                            {
                                section = values[0].Trim();
                                key = values[1].Trim();
                                value = values[2].Trim();
                            }
                            else
                                continue;

                            if (!String.IsNullOrWhiteSpace(section))
                                if (section.StartsWith(@""""))
                                    section = section.Substring(1, section.Length - 2);

                            if (!String.IsNullOrWhiteSpace(key))
                                if (key.StartsWith(@""""))
                                    key = key.Substring(1, key.Length - 2);

                            if (!String.IsNullOrWhiteSpace(value))
                                if (value.Trim().StartsWith(@""""))
                                    value = value.Substring(1, value.Length - 2);

                            if (props.Exists(section, key))
                                props.SetProperty(section, key, value);
                            else if (createIfNotFound)
                                props.AddProperty(section, key, value);

                        }
                    }
                }
            }

            if (settings != null)
            {
                foreach (KeyValuePair<String, String> setting in settings)
                {
                    String section = String.Empty;
                    String key = setting.Key;
                    String value = setting.Value;

                    System.Text.RegularExpressions.Match match = Regex.Match(setting.Key, @"^\[(.*?)\]\s*:\s*(.*?)\s*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        section = match.Groups[1].Value;
                        key = match.Groups[2].Value;
                    }

                    if (props.Exists(section, key))
                        props.SetProperty(section, key, value);
                    else if (createIfNotFound)
                        props.AddProperty(section, key, value);
                }
            }

            if (String.IsNullOrWhiteSpace(destinationFile))
                props.Save(sourceFile);
            else
                props.Save(destinationFile);
        }

        static public void XPath(String sourceFile, String destinationFile, String transformFile, List<KeyValuePair<String, String>> settings)
        {
            io.Stream transformStream = null;
            if (!String.IsNullOrWhiteSpace(transformFile))
                transformStream = new io.FileStream(transformFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            XPath(sourceFile, destinationFile, transformStream, settings);
        }

        static public void XPath(String sourceFile, String destinationFile, io.Stream transformFile, List<KeyValuePair<String, String>> settings)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(sourceFile);

            if (transformFile != null)
            {
                if (transformFile != null)
                {
                    using (io.StreamReader reader = new io.StreamReader(transformFile))
                    {
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            char[] delims = { ',' };
                            String[] values = line.Split(delims);

                            String key = null;
                            String value = null;

                            if (values.Length >= 2)
                            {
                                key = values[0].Trim();
                                value = values[1].Trim();
                            }
                            else
                                continue;

                            if (!String.IsNullOrWhiteSpace(key))
                                if (key.StartsWith(@""""))
                                    key = key.Substring(1, key.Length - 2);

                            if (!String.IsNullOrWhiteSpace(value))
                                if (value.Trim().StartsWith(@""""))
                                    value = value.Substring(1, value.Length - 2);

                            XmlNodeList nodes = doc.SelectNodes(key);
                            foreach (XmlNode node in nodes)
                                node.InnerText = value;

                        }
                    }
                }
            }
            
            foreach (KeyValuePair<String, String> setting in settings)
            {
                String localValue = "";
                if (setting.Value != null)
                {
                    localValue = setting.Value;
                }

                if (setting.Key != null)
                {
                    XmlNodeList nodes = doc.SelectNodes(setting.Key);
                    foreach (XmlNode node in nodes)
                        node.InnerText = localValue;
                }
            }

            if (String.IsNullOrWhiteSpace(destinationFile))
                doc.Save(sourceFile);
            else
                doc.Save(destinationFile);
        }

        static public void RegexMatch(String sourceFile, String destinationFile, String transformFile, List<KeyValuePair<String, String>> settings)
        {
            String[] lines = System.IO.File.ReadAllLines(sourceFile);
            String[] xformLines = null;

            if (transformFile != null)
                xformLines = System.IO.File.ReadAllLines(transformFile);

            for (int i = 0; i < lines.Length; i++)
            {
                // Apply Settings From Transform File
                if (xformLines != null)
                {
                    foreach (String xformLine in xformLines)
                    {
                        char[] delims = { ',' };
                        String[] values = xformLine.Split(delims);

                        String key = null;
                        String value = null;

                        if (values.Length >= 2)
                        {
                            key = values[0].Trim();
                            value = values[1].Trim();
                        }
                        else
                            continue;

                        if (Regex.IsMatch(lines[i], key))
                        {
                            lines[i] = DoRegexReplaceWithAutoSave(lines[i], key, value, RegexOptions.IgnoreCase);
                        }
                    }
                }

                // Apply Settings From Package
                foreach (KeyValuePair<String, String> setting in settings)
                {
                    if (Regex.IsMatch(lines[i], setting.Key))
                    {
                        if (setting.Value != null)
                        {
                            String localValue = setting.Value;
                            lines[i] = DoRegexReplaceWithAutoSave(lines[i], setting.Key, localValue, RegexOptions.IgnoreCase);
                        }
                        else
                            lines[i] = DoRegexReplaceWithAutoSave(lines[i], setting.Key, "", RegexOptions.IgnoreCase);
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(destinationFile))
                System.IO.File.WriteAllLines(sourceFile, lines);
            else
                System.IO.File.WriteAllLines(destinationFile, lines);
        }

        //
        //  This function will do a Regex.Replace, but will automatically keep any "match groups" in the 
        //  pattern if no match group variables are specified in the replacement string.  If match group
        //  variables exist in the replacement string, a regular Regex.Replace will be performed.
        //  
        //  Example:    DoRegexReplaceWithAutoSave("ABCDEFGHI", "ABCDEFGHI", "XXX")
        //  Result :    XXX
        //  Reason :    Performs normal Regex replacement since no match groups were specified.
        //
        //  Example:    DoRegexReplaceWithAutoSave("ABCDEFGHI", "(ABC)DEF(GHI)", "XXX")
        //  Result :    ABCXXXGHI
        //  Reason :    Auto-saves ABC and GHI because they are in groups and no specific group match variable was specified.
        //
        //  Example:    DoRegexReplaceWithAutoSave("ABCDEFGHI", "(ABC)DEF(GHI)", "${1}XXX")
        //  Result :    ABCXXX
        //  Reason :    Saves ABC only because a group match variable was specified (${1}) and normal Regex replacement occurs.
        //
        //
        static public String DoRegexReplaceWithAutoSave(String input, String pattern, String replacement, RegexOptions options)
        {
            System.Text.RegularExpressions.Match match = Regex.Match(input, pattern, options);
            bool autoSaveGroups = !(replacement.Contains("$0") || replacement.Contains("${0}"));

            String matchedString = match.Groups[0].Value;

            for (int i = 1; i < match.Groups.Count && autoSaveGroups; i++)
            {
                // If replacement string contains a match group variable (ex: $1 or ${1}), then stop processing and 
                // just perform a normal regular expression match.
                if (replacement.Contains("$" + i) || replacement.Contains("${" + i + "}"))
                {
                    autoSaveGroups = false;
                    break;
                }

                String matchGroup = match.Groups[i].Value;
                // Escape Any Regex Special Characters
                matchGroup = matchGroup.Replace(@"\", @"\\");
                matchGroup = matchGroup.Replace(@"^", @"\^");
                matchGroup = matchGroup.Replace(@"$", @"\$");
                matchGroup = matchGroup.Replace(@".", @"\.");
                matchGroup = matchGroup.Replace(@"|", @"\|");
                matchGroup = matchGroup.Replace(@"?", @"\?");
                matchGroup = matchGroup.Replace(@"*", @"\*");
                matchGroup = matchGroup.Replace(@"+", @"\+");
                matchGroup = matchGroup.Replace(@"(", @"\(");
                matchGroup = matchGroup.Replace(@")", @"\)");
                matchGroup = matchGroup.Replace(@"[", @"\[");
                matchGroup = matchGroup.Replace(@"]", @"\]");

                // Remove the match group value from the string you will eventually match on
                matchedString = Regex.Replace(matchedString, matchGroup + "?", "");
            }

            if (autoSaveGroups)
                return Regex.Replace(input, matchedString, replacement, options);
            else
                return Regex.Replace(input, pattern, replacement, options);

        }

    }
}
