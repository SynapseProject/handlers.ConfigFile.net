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

        public abstract SynapseDirectory Create(string childDirName = null);
        public abstract void Delete(string dirName = null);
        public abstract bool Exists(string dirName = null);

        public abstract IEnumerable<SynapseDirectory> GetDirectories();
        public abstract IEnumerable<SynapseFile> GetFiles();
        public abstract String PathCombine(params string[] paths);

        public void CopyTo(SynapseDirectory target, bool recurse = true, bool overwrite = true)
        {
            foreach ( SynapseDirectory childDir in GetDirectories() )
            {
                SynapseDirectory targetChild = target.Create( childDir.Name );
                if ( recurse )
                    childDir.CopyTo( targetChild );
            }

            foreach ( SynapseFile file in GetFiles())
            {
                String targetFileName = target.PathCombine( target.FullName, file.Name );
                SynapseFile targetFile = file.Create( targetFileName );
                file.CopyTo( targetFile );
            }

            Console.WriteLine( $"Copied Directory [{this.FullName}] to [{target.FullName}]." );

        }

        public void MoveTo(SynapseDirectory target, bool recurse = true, bool overwrite = true)
        {
            Console.WriteLine( $"Moved Directory [{this.FullName}] to [{target.FullName}]." );
        }

        public bool IsEmpty()
        {
            return (GetDirectories().Count() == 0 && GetFiles().Count() == 0);
        }

        public void Clear(string dirName = null)
        {
            foreach ( SynapseDirectory dir in GetDirectories() )
                dir.Delete();

            foreach ( SynapseFile file in GetFiles() )
                file.Delete();
        }


    }
}
