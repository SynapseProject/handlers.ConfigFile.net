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

        public abstract SynapseDirectory Create(string childDirName = null, String callbackLabel = null, Action<string, string> callback = null);
        public abstract void Delete(string dirName = null, String callbackLabel = null, Action<string, string> callback = null);
        public abstract bool Exists(string dirName = null);

        public abstract IEnumerable<SynapseDirectory> GetDirectories();
        public abstract IEnumerable<SynapseFile> GetFiles();
        public abstract String PathCombine(params string[] paths);

        public void CopyTo(SynapseDirectory target, bool recurse = true, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            foreach ( SynapseDirectory childDir in GetDirectories() )
            {
                String targetDirName = target.PathCombine(target.FullName, $"{childDir.Name}/");
                SynapseDirectory targetChild = target.Create(targetDirName);
                if (recurse)
                    childDir.CopyTo(targetChild, recurse, overwrite, callbackLabel, callback);
            }

            foreach (SynapseFile file in GetFiles())
            {
                String targetFileName = target.PathCombine(target.FullName, file.Name);
                SynapseFile targetFile = file.Create(targetFileName);
                file.CopyTo(targetFile, overwrite, callbackLabel, callback);
            }

            callback?.Invoke(callbackLabel,  $"Copied Directory [{this.FullName}] to [{target.FullName}]." );

        }

        public void MoveTo(SynapseDirectory target, bool recurse = true, bool overwrite = true, String callbackLabel = null, Action<string, string> callback = null)
        {
            foreach ( SynapseDirectory childDir in GetDirectories() )
            {
                SynapseDirectory targetChild = target.Create( childDir.Name );
                if ( recurse )
                    childDir.MoveTo( targetChild, recurse, overwrite, callbackLabel, callback );
                childDir.Delete();
            }

            foreach ( SynapseFile file in GetFiles() )
            {
                String targetFileName = target.PathCombine( target.FullName, file.Name );
                SynapseFile targetFile = file.Create( targetFileName );
                file.MoveTo( targetFile, overwrite, callbackLabel, callback );
            }

            callback?.Invoke( callbackLabel, $"Moved Directory [{this.FullName}] to [{target.FullName}]." );
        }

        public bool IsEmpty()
        {
            return (GetDirectories().Count() == 0 && GetFiles().Count() == 0);
        }

        public void Clear(string dirName = null, String callbackLabel = null, Action<string, string> callback = null)
        {
            foreach ( SynapseDirectory dir in GetDirectories() )
                dir.Delete(null, callbackLabel, callback);

            foreach ( SynapseFile file in GetFiles() )
                file.Delete(null, callbackLabel, callback);
        }


    }
}
