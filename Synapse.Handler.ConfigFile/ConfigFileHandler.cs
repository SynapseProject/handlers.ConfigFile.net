using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Synapse.Core;

namespace Synapse.Handlers.ConfigFile
{
    public class ConfigFileHandler : HandlerRuntimeBase
    {
        HandlerConfig config = null;
        HandlerParameters parameters = null;

        public override IHandlerRuntime Initialize(string configStr)
        {
            config = HandlerUtils.Deserialize<HandlerConfig>(configStr);
            return base.Initialize(configStr);
        }

        public override ExecuteResult Execute(HandlerStartInfo startInfo)
        {
            ExecuteResult result = null;
            if (startInfo.Parameters != null)
                parameters = HandlerUtils.Deserialize<HandlerParameters>(startInfo.Parameters);

            return result;
        }
    }
}
