using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using Alphaleonis.Win32.Filesystem;
using Zephyr.Filesystem;

using Synapse.Core;
using Synapse.Handlers.FileUtil;


public class DeleteFileHandler : HandlerRuntimeBase
{
    DeleteFileHandlerConfig config = null;
    DeleteFileHandlerParameters parameters = null;
    Clients clients = new Clients();

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<DeleteFileHandlerConfig>(configStr);
        clients.aws = HandlerUtils.InitAwsClient(config.Aws);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        DeleteFileHandlerConfig config = new DeleteFileHandlerConfig();

        config.Recurse = true;
        config.Verbose = true;
        config.StopOnError = true;

        return config;
    }

    public override object GetParametersInstance()
    {
        DeleteFileHandlerParameters parms = new DeleteFileHandlerParameters();

        parms.Targets = new List<string>();
        parms.Targets.Add(@"C:\MyDir\MyFile.txt");
        parms.Targets.Add(@"C:\MyDir\MySubDir\");
        parms.Targets.Add(@"\\server\share$\dir\file.dat");
        parms.Targets.Add(@"s3://mybucket/dir/subdir/");
        parms.Targets.Add(@"s3://mybucket/dir/dir2/MyFile.txt");

        return parms;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;
        int cheapSequence = 0;

        OnProgress("DeleteFileHandler", "Handler Execution Begins.", StatusType.Running, 0, cheapSequence++);
        try
        {
            if (startInfo.Parameters != null)
                parameters = HandlerUtils.Deserialize<DeleteFileHandlerParameters>(startInfo.Parameters);

            bool isValid = Validate();

            if (isValid)
            {
                if (parameters.Targets != null)
                {
                    OnLogMessage("DeleteFileHandler", $"Starting Delete Of [{string.Join(",", parameters.Targets.ToArray())}]");
                    foreach (String target in parameters.Targets)
                    {
                        if (Utilities.IsDirectory(target))
                        {
                            ZephyrDirectory dir = Utilities.GetZephyrDirectory(target, clients);
                            dir.Delete(config.Recurse, config.StopOnError, config.Verbose, "DeleteFileHandler", Logger);
                        }
                        else
                        {
                            ZephyrFile file = Utilities.GetZephyrFile(target, clients);
                            file.Delete(config.StopOnError, config.Verbose, "DeleteFileHandler", Logger);
                        }
                    }
                    OnLogMessage("DeleteFileHandler", $"Finished Delete Of [{string.Join(",", parameters.Targets.ToArray())}]");
                }
            }
            else
            {
                OnLogMessage("DeleteFileHandler", "Validation Failed.", LogLevel.Error);
                throw new Exception("Invalid Input Received");
            }
        }
        catch (Exception e)
        {
            OnProgress("DeleteFileHandler", "Handler Execution Failed.", StatusType.Failed, 0, cheapSequence++, false, e);
            throw e;
        }

        OnProgress("DeleteFileHandler", "Handler Execution Completed.", StatusType.Complete, 0, cheapSequence++);
        return result;
    }

    private bool Validate()
    {
        bool isValid = true;
        HashSet<UrlType> urlTypes = new HashSet<UrlType>();
        if (parameters.Targets != null)
        {
            foreach (String target in parameters.Targets)
                urlTypes.Add(Utilities.GetUrlType(target));

            if (config.Aws == null && (urlTypes.Contains(UrlType.AwsS3Directory) || urlTypes.Contains(UrlType.AwsS3File)))
            {
                OnLogMessage("Validate", "Aws Config Section Required When One Or More Endpoints Are Amazon S3 Buckets.");
                isValid = false;
            }

        }

        return isValid;
    }

    private void Logger(String context, String message)
    {
        OnLogMessage(context, message);
    }
}
