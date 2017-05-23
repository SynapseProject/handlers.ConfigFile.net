using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using Alphaleonis.Win32.Filesystem;

using Synapse.Core;
using Synapse.Handlers.FileUtil;


public class DeleteFileHandler : HandlerRuntimeBase
{
    DeleteFileHandlerConfig config = null;
    DeleteFileHandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<DeleteFileHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        DeleteFileHandlerConfig config = new DeleteFileHandlerConfig();

        config.Recursive = true;
        config.IgnoreReadOnly = true;
        config.UseTransaction = false;
        config.FailIfMissing = true;
        config.Verbose = true;

        return config;
    }

    public override object GetParametersInstance()
    {
        DeleteFileHandlerParameters parms = new DeleteFileHandlerParameters();

        parms.Targets = new List<string>();
        parms.Targets.Add(@"C:\MyDir\MyFile.txt");
        parms.Targets.Add(@"C:\MyDir\MySubDir\");
        parms.Targets.Add(@"\\server\share$\dir\file.dat");

        return parms;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        if (startInfo.Parameters != null)
            parameters = HandlerUtils.Deserialize<DeleteFileHandlerParameters>(startInfo.Parameters);

        bool isValid = Validate();

        if (isValid)
        {
            if (parameters.Targets != null)
            {                
                DeleteUtil util = new DeleteUtil(config);
                if (config.UseTransaction)
                    util.Transaction.Start();

                foreach (String target in parameters.Targets)
                {
                    util.Delete(target, "Delete", Logger, startInfo.IsDryRun);
                }

                if (config.UseTransaction)
                    util.Transaction.Stop();

            }
        }
        else
            throw new Exception("Invalid Input Received");

        return result;
    }

    private bool Validate()
    {
        bool isValid = true;
        if (parameters.Targets != null)
        {
            if (config.UseTransaction)
            {
                foreach (String target in parameters.Targets)
                {
                    DriveInfo drive = new DriveInfo(target);
                    if (drive.IsUnc)
                    {
                        OnLogMessage("Validate", "UseTransaction Not Supported On [" + drive.DriveType + "] Drives.  [" + target + "]");
                        isValid = false;
                    }
                }
            }
        }

        return isValid;
    }

    private void Logger(String context, String message)
    {
        OnLogMessage(context, message);
    }


}
