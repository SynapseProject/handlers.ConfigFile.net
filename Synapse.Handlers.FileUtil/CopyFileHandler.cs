using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using Alphaleonis.Win32.Filesystem;

using Synapse.Handlers.FileUtil;

using Synapse.Core;

public class CopyFileHandler : HandlerRuntimeBase
{
    CopyFileHandlerConfig config = null;
    CopyFileHandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = HandlerUtils.Deserialize<CopyFileHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        CopyFileHandlerConfig config = new CopyFileHandlerConfig();

        config.Action = FileAction.Copy;
        config.OverwriteExisting = true;
        config.IncludeSubdirectories = true;
        config.PurgeDestination = false;
        config.UseTransaction = false;
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
        fs1.Destinations.Add(@"C:\MyDest\");
        parms.FileSets.Add(fs1);

        return parms;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;
        if (startInfo.Parameters != null)
            parameters = HandlerUtils.Deserialize<CopyFileHandlerParameters>(startInfo.Parameters);

        bool isValid = Validate();

        if (isValid)
        {
            CopyUtil util = new CopyUtil(config);

            if (parameters.FileSets != null)
            {
                if (config.UseTransaction)
                    util.Transaction.Start();

                foreach (FileSet set in parameters.FileSets)
                {
                    if (set != null && set.Sources != null && set.Destinations != null)
                    {
                        foreach (String source in set.Sources)
                            foreach (String destination in set.Destinations)
                            {
                                if (config.Action == FileAction.Copy)
                                    util.Copy(source, destination, "Copy", Logger, startInfo.IsDryRun);
                                else
                                    util.Move(source, destination, "Move", Logger, startInfo.IsDryRun);
                            }
                    }
                }

                if (config.UseTransaction)
                    util.Transaction.Stop();
            }
        }
        else
            throw new Exception("Invalid Input Received");

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
                if (set.Sources != null)
                {
                    if (config.UseTransaction)
                    {
                        foreach (String source in set.Sources)
                        {
                            DriveInfo drive = new DriveInfo(source);
                            if (drive.IsUnc)
                            {
                                OnLogMessage("Validate", "UseTransaction Not Supported On [" + drive.DriveType + "] Drives.  [" + source + "]");
                                isValid = false;
                            }
                        }
                    }
                }

                if (set.Destinations != null)
                {
                    if (config.Action == FileAction.Move && set.Destinations.Count > 1)
                    {
                        OnLogMessage("Validate", "Cannot Have Multiple Destinations On A Move Action");
                        isValid = false;
                    }

                    if (config.UseTransaction)
                    {
                        foreach (String destination in set.Destinations)
                        {
                            DriveInfo drive = new DriveInfo(destination);
                            if (drive.IsUnc)
                            {
                                OnLogMessage("Validate", "UseTransaction Not Supported On [" + drive.DriveType + "] Drives.  [" + destination + "]");
                                isValid = false;
                            }
                        }
                    }
                }


            }
        }

        return isValid;
    }
}

