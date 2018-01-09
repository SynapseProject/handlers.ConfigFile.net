using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Synapse.Filesystem
{
    public abstract class SynapseFile
    {
        public abstract String Name { get; }
        public abstract String FullName { get; set; }

        public SynapseFile() { }

        public SynapseFile(string fullName)
        {
            FullName = fullName;
        }

        public abstract SynapseFile Create(string fileName = null, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string fileName = null, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string fileName = null);

        public abstract SynapseDirectory CreateDirectory(string dirName, String callbackLabel = null, Action<string, string> callback = null);

        public abstract Stream OpenStream(AccessType access, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void CloseStream(String callbackLabel = null, Action<string, string> callback = null);

        public void CopyTo(SynapseFile file, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (file.Exists() && !overwrite)
                    throw new Exception($"File [{file.FullName}] Already Exists.");

                Stream source = this.OpenStream(AccessType.Read);
                Stream target = file.OpenStream(AccessType.Write);

                source.CopyTo(target);

                this.CloseStream();
                file.CloseStream();

                if (verbose)
                    Logger.Log($"Copied File [{this.FullName}] to [{file.FullName}].", callbackLabel, callback);
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                if (stopOnError)
                    throw;
            }
        }

        public void MoveTo(SynapseFile file, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            try
            {
                if (file.Exists() && !overwrite)
                    throw new Exception($"File [{file.FullName}] Already Exists.");

                CopyTo(file, overwrite, false);
                this.Delete(verbose: false);
                if (verbose)
                    Logger.Log($"Moved File [{this.FullName}] to [{file.FullName}].", callbackLabel, callback);
            }
            catch (Exception e)
            {
                Logger.Log($"ERROR - {e.Message}", callbackLabel, callback);
                if (stopOnError)
                    throw;
            }
        }

        public void CopyTo(SynapseDirectory dir, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            String targetFilePath = dir.PathCombine(dir.FullName, this.Name);
            SynapseFile targetFile = dir.CreateFile(targetFilePath);
            CopyTo(targetFile, overwrite, stopOnError, false, callbackLabel, callback);
            if (verbose)
                Logger.Log($"Copied File [{this.FullName}] to [{dir.FullName}].", callbackLabel, callback);
        }

        public void MoveTo(SynapseDirectory dir, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            CopyTo(dir, overwrite, stopOnError, false,callbackLabel, callback);
            this.Delete(verbose: false);
            if (verbose)
                Logger.Log($"Moved File [{this.FullName}] to [{dir.FullName}].", callbackLabel, callback);
        }
    }
}

