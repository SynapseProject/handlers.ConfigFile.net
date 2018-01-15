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
using Synapse.Filesystem;
using Synapse.Handlers.FileUtil;

public class ModifyFileHandler : HandlerRuntimeBase
{
    ModifyFileHandlerConfig config = null;
    ModifyFileHandlerParameters parameters = null;
    SynapseClients clients = new SynapseClients();

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<ModifyFileHandlerConfig>(configStr);
        clients.aws = HandlerUtils.InitAwsClient(config.Aws);
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
        int cheapSequence = 0;

        try
        {
            OnProgress("ModifyFileHandler", "Handler Execution Begins.", StatusType.Running, 0, cheapSequence++);
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
        }
        catch (Exception e)
        {
            OnProgress("ModifyFileHandler", "Handler Execution Failed.", StatusType.Failed, 0, cheapSequence++, false, e);
            if (e.InnerException != null)
                OnProgress("ModifyFileHandler", "Inner Exception.", StatusType.Failed, 0, cheapSequence++, false, e.InnerException);
            throw e;
        }

        OnProgress("ModifyFileHandler", "Handler Execution Ends.", StatusType.Running, 0, cheapSequence++);
        return result;
    }

    private void ProcessFile(ModifyFileType file, HandlerStartInfo startInfo)
    {
        bool createIfMissing = config.CreateSettingIfNotFound;
        if (file.CreateSettingIfNotFound.HasValue)
            createIfMissing = file.CreateSettingIfNotFound.Value;

        bool overwrite = config.OverwriteExisting;
        if (file.OverwriteExisting.HasValue)
            overwrite = file.OverwriteExisting.Value;

        ConfigType modifyType = config.Type;
        if (file.Type != ConfigType.None)
            modifyType = file.Type;

        if (config.BackupSource)
        {
            try
            {
                SynapseFile sourceFile = Utilities.GetSynapseFile(file.Source, clients);
                SynapseFile backupFile = Utilities.GetSynapseFile($"{file.Source}_{DateTime.Now.ToString("yyyyMMddHHmmss")}", clients);
                sourceFile.CopyTo(backupFile);
            }
            catch (Exception e)
            {
                OnLogMessage("BackupSource", $"Error When Backing Up Source File [{file.Source}].", config.StopOnError ? LogLevel.Error : LogLevel.Warn, e);
                if (config.StopOnError)
                    throw;
            }
        }

        try
        {
            Stream settingsFileStream = GetSettingsFileStream(modifyType, file.SettingsFile, startInfo.Crypto);

            switch (modifyType)
            {
                case ConfigType.KeyValue:
                    Munger.KeyValue(PropertyFile.Type.Java, file.Source, file.Destination, settingsFileStream, file.SettingsKvp, createIfMissing, overwrite, clients);
                    break;
                case ConfigType.INI:
                    Munger.KeyValue(PropertyFile.Type.Ini, file.Source, file.Destination, settingsFileStream, file.SettingsKvp, createIfMissing, overwrite, clients);
                    break;
                case ConfigType.Regex:
                    Munger.RegexMatch(file.Source, file.Destination, settingsFileStream, file.SettingsKvp, overwrite, clients);
                    break;
                case ConfigType.XmlTransform:
                    Munger.XMLTransform(file.Source, file.Destination, settingsFileStream, overwrite, clients);
                    break;
                case ConfigType.XPath:
                    Munger.XPath(file.Source, file.Destination, settingsFileStream, file.SettingsKvp, overwrite, clients);
                    break;
                default:
                    String message = "Unknown File Type [" + modifyType + "] Received.";
                    OnLogMessage("ProcessFile", message);
                    throw new Exception(message);
            }

            if (String.IsNullOrWhiteSpace(file.Destination))
                OnLogMessage("ModifyFileHandler", String.Format(@"Config Type [{0}], Modified [{1}].", modifyType, file.Source));
            else
                OnLogMessage("ModifyFileHandler", String.Format(@"Config Type [{0}], Modified [{1}] to [{2}].", modifyType, file.Source, file.Destination));
        }
        catch (Exception e)
        {
            OnLogMessage(config.Type.ToString(), $"Error Modifying File [{file.Source}].", (config.StopOnError == true) ? LogLevel.Error : LogLevel.Warn, e);
            if (config.StopOnError)
                throw;
        }
    }

    private bool Validate()
    {
        bool isValid = true;
        if (parameters.Files != null)
        {
            foreach (ModifyFileType file in parameters.Files)
            {
                if (config.Aws == null && Utilities.GetUrlType(file.Source) == UrlType.AwsS3File)
                {
                    OnLogMessage("Validate", $"File [{file.Source}] Is In An S3 Bucket, But No Aws Section Is Specified In The Config Section.");
                    isValid = false;
                }

                if (config.Aws == null && Utilities.GetUrlType(file.Destination) == UrlType.AwsS3File)
                {
                    OnLogMessage("Validate", $"File [{file.Destination}] Is In An S3 Bucket, But No Aws Section Is Specified In The Config Section.");
                    isValid = false;
                }

                if (config.Aws == null && Utilities.GetUrlType(file.SettingsFile.Name) == UrlType.AwsS3File)
                {
                    OnLogMessage("Validate", $"File [{file.SettingsFile.Name}] Is In An S3 Bucket, But No Aws Section Is Specified In The Config Section.");
                    isValid = false;
                }

            }
        }

        return isValid;
    }

    private Stream GetSettingsFileStream(ConfigType type, SettingsFileType settings, CryptoProvider planCrypto)
    {
        Stream stream = null;
        if (String.IsNullOrWhiteSpace(settings.Name))
            stream = null;
        else
        {
            SynapseFile settingsFile = Utilities.GetSynapseFile(settings.Name, clients);
            stream = settingsFile.OpenStream(AccessType.Read);
            if (settings.HasEncryptedValues)
            {
                CryptoProvider crypto = new CryptoProvider();
                crypto.Key = new CryptoKeyInfo();
                if (planCrypto != null)
                {
                    crypto.Key.Uri = planCrypto.Key.Uri;
                    crypto.Key.ContainerName = planCrypto.Key.ContainerName;
                    crypto.Key.CspFlags = planCrypto.Key.CspFlags;
                }

                if (!String.IsNullOrWhiteSpace(settings.Crypto?.Key?.Uri))
                    crypto.Key.Uri = settings.Crypto.Key.Uri;
                if (!String.IsNullOrWhiteSpace(settings.Crypto?.Key?.ContainerName))
                    crypto.Key.ContainerName = settings.Crypto.Key.ContainerName;

                if (String.IsNullOrWhiteSpace(crypto.Key.Uri) || String.IsNullOrWhiteSpace(crypto.Key.ContainerName))
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

