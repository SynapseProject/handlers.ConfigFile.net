using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Synapse.Filesystem
{
    public abstract class SynapseDirectory
    {
        public abstract String FullName { get; set; }

        public abstract String Name { get; }
        public abstract String Parent { get; }
        public abstract String Root { get; }

        public abstract SynapseDirectory Create(string childDirName = null, bool failIfExists = false, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string dirName = null, bool recurse = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string dirName = null);

        public abstract SynapseFile CreateFile(string fullName, String callbackLabel = null, Action<string, string> callback = null);

        public abstract IEnumerable<SynapseDirectory> GetDirectories();
        public abstract IEnumerable<SynapseFile> GetFiles();
        public abstract String PathCombine(params string[] paths);

        public void CopyTo(SynapseDirectory target, bool recurse = true, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (this.Exists())
            {
                foreach (SynapseDirectory childDir in GetDirectories())
                {
                    try
                    {
                        String targetDirName = target.PathCombine(target.FullName, $"{childDir.Name}/");
                        SynapseDirectory targetChild = target.Create(targetDirName);
                        if (recurse)
                            childDir.CopyTo(targetChild, recurse, overwrite, verbose, stopOnError, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                foreach (SynapseFile file in GetFiles())
                {
                    try
                    {
                        String targetFileName = target.PathCombine(target.FullName, file.Name);
                        SynapseFile targetFile = target.CreateFile(targetFileName, callbackLabel, callback);
                        file.CopyTo(targetFile, overwrite, stopOnError, verbose, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                if (verbose)
                    Logger.Log($"Copied Directory [{this.FullName}] to [{target.FullName}].", callbackLabel, callback);
            }
            else
            {
                string message = $"[{this.FullName}] Does Not Exist.";
                Logger.Log(message, callbackLabel, callback);
                if (stopOnError)
                    throw new Exception(message);
            }
        }

        public void MoveTo(SynapseDirectory target, bool overwrite = true, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            if (this.Exists())
            {
                foreach (SynapseDirectory childDir in GetDirectories())
                {
                    try
                    {
                        SynapseDirectory targetChild = target.Create(childDir.Name);
                        childDir.MoveTo(targetChild, overwrite, stopOnError, verbose, callbackLabel, callback);
                        childDir.Delete(verbose: false);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                foreach (SynapseFile file in GetFiles())
                {
                    try
                    {
                        String targetFileName = target.PathCombine(target.FullName, file.Name);
                        SynapseFile targetFile = file.Create(targetFileName, overwrite);
                        file.MoveTo(targetFile, stopOnError, overwrite, verbose, callbackLabel, callback);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message, callbackLabel, callback);
                        if (stopOnError)
                            throw;
                    }
                }

                if (verbose)
                    Logger.Log($"Moved Directory [{this.FullName}] to [{target.FullName}].", callbackLabel, callback);
            }
            else
            {
                string message = $"[{this.FullName}] Does Not Exist.";
                Logger.Log(message, callbackLabel, callback);
                if (stopOnError)
                    throw new Exception(message);
            }
        }

        public bool IsEmpty()
        {
            return (GetDirectories().Count() == 0 && GetFiles().Count() == 0);
        }

        public void Clear(string dirName = null, bool stopOnError = true, bool verbose = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            foreach ( SynapseDirectory dir in GetDirectories() )
                dir.Delete(null, true, stopOnError, verbose, callbackLabel, callback);

            foreach ( SynapseFile file in GetFiles() )
                file.Delete(null, stopOnError, verbose, callbackLabel, callback);
        }


    }
}
