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
                        ProcessFile(file, startInfo);
                else
                    Parallel.ForEach(parameters.Files, file => ProcessFile(file, startInfo));
            }
        }

        return result;
    }

    private void ProcessFile(ModifyFileType file, HandlerStartInfo startInfo)
    {
        bool createIfMissing = config.CreateSettingIfNotFound;
        if (file.CreateSettingIfNotFound.HasValue)
            createIfMissing = file.CreateSettingIfNotFound.Value;

        if (config.BackupSource)
        {
            String backupFile = Path.GetFileNameWithoutExtension(file.Source) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.Source);
            String backupPath = Path.Combine(Path.GetDirectoryName(file.Source), backupFile);
            File.Copy(file.Source, backupPath, true);
        }

        Stream settingsFileStream = GetSettingsFileStream(config.Type, file.SettingsFile, startInfo.Crypto);

        switch (config.Type)
        {
            case ConfigType.KeyValue:
                Munger.KeyValue(PropertyFile.Type.Java, file.Source, file.Destination, settingsFileStream, file.SettingsKvp, createIfMissing);
                break;
            case ConfigType.INI:
                Munger.KeyValue(PropertyFile.Type.Ini, file.Source, file.Destination, settingsFileStream, file.SettingsKvp, createIfMissing);
                break;
            case ConfigType.Regex:
                Munger.RegexMatch(file.Source, file.Destination, settingsFileStream, file.SettingsKvp);
                break;
            case ConfigType.XmlTransform:
                Munger.XMLTransform(file.Source, file.Destination, settingsFileStream);
                break;
            case ConfigType.XPath:
                Munger.XPath(file.Source, file.Destination, settingsFileStream, file.SettingsKvp);
                break;
            default:
                String message = "Unknown File Type [" + config.Type + "] Received.";
                OnLogMessage("ProcessFile", message);
                throw new Exception(message);
        }

        if (String.IsNullOrWhiteSpace(file.Destination))
            OnLogMessage("ModifyFileHandler", String.Format(@"Config Type [{0}], Modified [{1}].", config.Type, file.Source));
        else
            OnLogMessage("ModifyFileHandler", String.Format(@"Config Type [{0}], Modified [{1}] to [{2}].", config.Type, file.Source, file.Destination));

    }

    private bool Validate()
    {
        return true;
    }

    private Stream GetSettingsFileStream(ConfigType type, SettingsFileType settings, CryptoProvider planCrypto)
    {
        Stream stream = null;
        if (String.IsNullOrWhiteSpace(settings.Name))
            stream = null;
        else
        {
            stream = new FileStream(settings.Name, FileMode.Open, FileAccess.Read);
            if (settings.HasEncryptedValues)
            {
                CryptoProvider crypto = new CryptoProvider();
                if (planCrypto != null)
                {
                    crypto.KeyUri = planCrypto.KeyUri;
                    crypto.KeyContainerName = planCrypto.KeyContainerName;
                    crypto.CspFlags = planCrypto.CspFlags;
                }

                if (!String.IsNullOrWhiteSpace(settings.Crypto?.KeyUri))
                    crypto.KeyUri = settings.Crypto.KeyUri;
                if (!String.IsNullOrWhiteSpace(settings.Crypto?.KeyContainerName))
                    crypto.KeyContainerName = settings.Crypto.KeyContainerName;

                if (String.IsNullOrWhiteSpace(crypto.KeyUri) || String.IsNullOrWhiteSpace(crypto.KeyContainerName))
                    OnLogMessage("SettingsFile", "WARNING : HasEncryptedValues flag is set, but no Crypto section was found in the plan.  No decryption will occur.");
                else
                {
                    crypto.LoadRsaKeys();
                    String settingsFileContent = null;

                    switch (type)
                    {
                        case ConfigType.XmlTransform:
                            settingsFileContent = CryptoUtils.DecryptXmlFile(stream, crypto);
                            break;
                        default:
                            settingsFileContent = CryptoUtils.DecryptCsvFile(stream, crypto);
                            break;
                    }
                    byte[] byteArray = new byte[0];
                    if (!String.IsNullOrWhiteSpace(settingsFileContent))
                        byteArray = Encoding.UTF8.GetBytes(settingsFileContent);
                    stream = new MemoryStream(byteArray);
                }
            }
        }

        return stream;
    }
}

