using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Synapse.Core;
using Synapse.Handlers.FileUtil;

public class ModifyFileHandler : HandlerRuntimeBase
{
    ModifyFileHandlerConfig config = null;
    ModifyFileHandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<ModifyFileHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        return null;
    }

    public override object GetParametersInstance()
    {
        ModifyFileHandlerParameters parms = new ModifyFileHandlerParameters();

        parms.Files = new List<ModifyFileType>();

        ModifyFileType file = new ModifyFileType();
        file.Source = @"C:\Temp\file.config";
        file.Destination = @"C:\Temp\file2.config";
        file.Settings = new List<SettingsPair<string, string>>();

        file.Settings.Add(new SettingsPair<string, string>("MyKey", "MyValue"));
        file.Settings.Add(new SettingsPair<string, string>("MyKey2", "MyValue2"));

        parms.Files.Add(file);

        return parms;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        if (startInfo.Parameters != null)
            parameters = HandlerUtils.Deserialize<ModifyFileHandlerParameters>(startInfo.Parameters);

        bool isValid = Validate();

        if (isValid)
        {
            if (parameters.Files != null)
            {
                if (config.RunSequential || parameters.Files.Count == 1)
                    foreach (ModifyFileType file in parameters.Files)
                        ProcessFile(file);
                else
                    Parallel.ForEach(parameters.Files, file => ProcessFile(file));
            }
        }

        return result;
    }

    private void ProcessFile(ModifyFileType file)
    {
        bool createIfMissing = config.CreateSettingIfNotFound;
        if (file.CreateSettingIfNotFound.HasValue)
            createIfMissing = file.CreateSettingIfNotFound.Value;

        switch (config.Type)
        {
            case ConfigType.KeyValue:
                Munger.KeyValue(PropertyFile.Type.Java, file.Source, file.Destination, file.SettingsFile, file.SettingsKvp, createIfMissing);
                break;
            case ConfigType.INI:
                Munger.KeyValue(PropertyFile.Type.Ini, file.Source, file.Destination, file.SettingsFile, file.SettingsKvp, createIfMissing);
                break;
            case ConfigType.Regex:
                Munger.RegexMatch(file.Source, file.Destination, file.SettingsFile, file.SettingsKvp);
                break;
            case ConfigType.XmlTransform:
                Munger.XMLTransform(file.Source, file.Destination, file.SettingsFile);
                break;
            case ConfigType.XPath:
                Munger.XPath(file.Source, file.Destination, file.SettingsFile, file.SettingsKvp);
                break;
            default:
                OnLogMessage("ProcessFile", "Unknown File Type [" + config.Type + "] Received.");
                break;
        }
    }

    private bool Validate()
    {
        return true;
    }
}

