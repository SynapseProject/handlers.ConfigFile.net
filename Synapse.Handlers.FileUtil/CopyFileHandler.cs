using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using Alphaleonis.Win32.Filesystem;

using Synapse.Filesystem;
using Synapse.Handlers.FileUtil;

using Synapse.Core;

public class CopyFileHandler : HandlerRuntimeBase
{
    CopyFileHandlerConfig config = null;
    CopyFileHandlerParameters parameters = null;
    int cheapSequence = 0;
    SynapseClients clients = new SynapseClients();

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<CopyFileHandlerConfig>(configStr);
        clients.aws = HandlerUtils.InitAwsClient(config.Aws);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        CopyFileHandlerConfig config = new CopyFileHandlerConfig();

        config.Action = FileAction.Copy;
        config.OverwriteExisting = true;
        config.Recurse = true;
        config.StopOnError = true;
        config.PurgeDestination = true;
        config.Verbose = true;

        return config;
    }

    public override object GetParametersInstance()
    {
        CopyFileHandlerParameters parms = new CopyFileHandlerParameters();

        parms.FileSets = new List<FileSet>();

        FileSet fs1 = new FileSet();
        fs1.Sources = new List<string>();
        fs1.Destinations = new List<string>();
        fs1.Sources.Add(@"C:\MyDir\MyFile.txt");
        fs1.Sources.Add(@"C:\MyDir\MySubDir\");
        fs1.Sources.Add(@"\\server\share$\Dir001");
        fs1.Sources.Add(@"s3://mybucket/dir001/");
        fs1.Sources.Add(@"s3://mybucket/dir002/MyFile.txt");
        fs1.Destinations.Add(@"C:\MyDest\");
        fs1.Destinations.Add(@"s3://mybucket/destdir/");
        parms.FileSets.Add(fs1);

        return parms;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        OnProgress("CopyFileHandler", "Handler Execution Begins.", StatusType.Running, 0, cheapSequence++);
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        try
        {
            if (startInfo.Parameters != null)
                parameters = HandlerUtils.Deserialize<CopyFileHandlerParameters>(startInfo.Parameters);

            bool isValid = Validate();

            if (isValid)
            {
                if (parameters.FileSets != null)
                {
                    foreach (FileSet set in parameters.FileSets)
                    {
                        if (set != null && set.Sources != null && set.Destinations != null)
                        {
                            OnLogMessage("CopyFileHandler", $"Starting {config.Action} From [{string.Join(",", set.Sources.ToArray())}] To [{string.Join(",", set.Destinations)}].");
                            foreach (String destination in set.Destinations)
                            {
                                if (Utilities.IsDirectory(destination) && config.PurgeDestination)
                                {
                                    SynapseDirectory clearDir = Utilities.GetSynapseDirectory(destination, clients);
                                    clearDir.Clear(null, config.StopOnError, config.Verbose, "Purge", Logger);
                                    OnLogMessage("CopyFileHandler", $"Directory [{destination}] Was Purged.");
                                }

                                foreach (String source in set.Sources)
                                {
                                    if (Utilities.IsDirectory(source))
                                    {
                                        SynapseDirectory sourceDir = Utilities.GetSynapseDirectory(source, clients);
                                        if (Utilities.IsDirectory(destination))
                                        {
                                            // Copy/Move Directory To Directory
                                            SynapseDirectory destDir = Utilities.GetSynapseDirectory(destination, clients);
                                            if (config.Action == FileAction.Copy)
                                                sourceDir.CopyTo(destDir, config.Recurse, config.OverwriteExisting, config.StopOnError, config.Verbose, "Copy", Logger);
                                            else
                                                sourceDir.MoveTo(destDir, config.OverwriteExisting, config.StopOnError, config.Verbose, "Move", Logger);
                                        }
                                        else
                                        {
                                            // This should never occur, as this scenario is addressed in "Validate".
                                            throw new Exception($"Can Not Copy Directory [{source}] To File [{destination}]");
                                        }
                                    }
                                    else
                                    {
                                        SynapseFile sourceFile = Utilities.GetSynapseFile(source, clients);
                                        if (Utilities.IsDirectory(destination))
                                        {
                                            // Copy/Move File To Directory
                                            SynapseDirectory destDir = Utilities.GetSynapseDirectory(destination, clients);
                                            if (config.Action == FileAction.Copy)
                                                sourceFile.CopyTo(destDir, config.OverwriteExisting, config.StopOnError, config.Verbose, "Copy", Logger);
                                            else
                                                sourceFile.MoveTo(destDir, config.OverwriteExisting, config.StopOnError, config.Verbose, "Move", Logger);
                                        }
                                        else
                                        {
                                            // Copy/Move File To File
                                            SynapseFile destFile = Utilities.GetSynapseFile(destination, clients);
                                            if (config.Action == FileAction.Copy)
                                                sourceFile.CopyTo(destFile, config.OverwriteExisting, config.StopOnError, config.Verbose, "Copy", Logger);
                                            else
                                                sourceFile.MoveTo(destFile, config.OverwriteExisting, config.StopOnError, config.Verbose, "Move", Logger);
                                        }
                                    }
                                }
                            }
                            OnLogMessage("CopyFileHandler", $"Finished {config.Action} From [{string.Join(",", set.Sources.ToArray())}] To [{string.Join(",", set.Destinations)}].");
                        }
                    }
                }
            }
            else
            {
                OnLogMessage("CopyFileHandler", "Validation Failed.", LogLevel.Error);
                throw new Exception("Validation Failed.");
            }
        }
        catch (Exception e)
        {
            OnProgress("CopyFileHandler", "Handler Execution Failed.", StatusType.Failed, 0, cheapSequence++, false, e);
            throw e;
        }

        OnProgress("CopyFileHandler", "Handler Execution Completed.", StatusType.Complete, 0, cheapSequence++);
        return result;
    }

    private void Logger(String context, String message)
    {
        OnLogMessage(context, message);
    }

    private bool Validate()
    {
        bool isValid = true;
        if (parameters.FileSets != null)
        {
            foreach (FileSet set in parameters.FileSets)
            {
                if (set.Destinations != null)
                {
                    if (config.Action == FileAction.Move && set.Destinations.Count > 1)
                    {
                        OnLogMessage("Validate", "Cannot Have Multiple Destinations On A Move Action");
                        isValid = false;
                    }
                }

                bool sourceHasDirectory = false;
                bool destinationHasFile = false;
                HashSet<UrlType> urlTypes = new HashSet<UrlType>();

                foreach (String source in set.Sources)
                {
                    urlTypes.Add(Utilities.GetUrlType(source));
                    if (Utilities.IsDirectory(source))
                        sourceHasDirectory = true;
                }

                foreach (String destination in set.Destinations)
                {
                    urlTypes.Add(Utilities.GetUrlType(destination));
                    if (Utilities.IsFile(destination))
                        destinationHasFile = true;
                }

                if (sourceHasDirectory && destinationHasFile)
                {
                    OnLogMessage("Validate", "Can Not Copy A Source Directory Into A Destination File.");
                    isValid = false;
                }

                if (config.Aws == null && (urlTypes.Contains(UrlType.AwsS3Directory) || urlTypes.Contains(UrlType.AwsS3File)))
                {
                    OnLogMessage("Validate", "Aws Config Section Required When One Or More Endpoints Are Amazon S3 Buckets.");
                    isValid = false;
                }
            }
        }

        return isValid;
    }

}

