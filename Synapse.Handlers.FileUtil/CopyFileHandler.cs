using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

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
        //TODO : Implement Me
        throw new NotImplementedException();
    }

    public override object GetParametersInstance()
    {
        //TODO : Implement Me
        throw new NotImplementedException();
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
                    util.StartTransaction();

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
                    util.StopTransaction();
            }
        }
        else
            throw new Exception("Invalid Input Received");

        return result;
    }

    public void Logger(String context, String message)
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
            }
        }

        return isValid;
    }
}

